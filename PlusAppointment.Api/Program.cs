using System.Text;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PlusAppointment.Data;
using PlusAppointment.Middleware;
using PlusAppointment.Repositories.Implementation.UserRepo;
using PlusAppointment.Repositories.Interfaces.UserRepo;
using PlusAppointment.Services.Interfaces.UserService;
using PlusAppointment.Repositories.Implementation.BusinessRepo;
using PlusAppointment.Repositories.Implementation.CustomerRepo;
using PlusAppointment.Repositories.Implementation.ServicesRepo;
using PlusAppointment.Repositories.Implementation.StaffRepo;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Services.Interfaces.BusinessService;
using PlusAppointment.Services.Interfaces.CustomerService;
using PlusAppointment.Services.Interfaces.ServicesService;
using PlusAppointment.Services.Interfaces.StaffService;
using StackExchange.Redis;
using PlusAppointment.Repositories.Implementation.AppointmentRepo;
using PlusAppointment.Services.Implementations.AppointmentService;
using PlusAppointment.Services.Implementations.BusinessService;
using PlusAppointment.Services.Implementations.CustomerService;
using PlusAppointment.Services.Implementations.ServicesService;
using PlusAppointment.Services.Implementations.StaffService;
using PlusAppointment.Services.Implementations.UserService;
using PlusAppointment.Utils.Redis;
using PlusAppointment.Utils.SendingEmail;
using PlusAppointment.Utils.SendingSms;
using Hangfire;
using Hangfire.MemoryStorage;

using PlusAppointment.Repositories.Implementation.CalculateMoneyRepo;
using PlusAppointment.Repositories.Implementation.ServiceCategoryRepo;
using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;
using PlusAppointment.Services.Implementations.CalculateMoneyService;
using PlusAppointment.Services.Implementations.ServiceCategoryService;
using PlusAppointment.Services.Interfaces.CalculateMoneyService;
using PlusAppointment.Services.Interfaces.ServiceCategoryService;
using PlusAppointment.Utils.Hub;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Ensure the Logs directory exists
EnsureLogsDirectory();
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddSignalR(); // Add SignalR services



// Configure the DbContext with the connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IServicesRepository, ServicesRepository>();
builder.Services.AddScoped<IServicesService, ServicesService>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICalculateMoneyRepo, CalculateMoneyRepo>();
builder.Services.AddScoped<ICalculateMoneyService, CalculateMoneyService>();
builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
builder.Services.AddScoped<IServiceCategoryRepo, ServiceCategoryRepo>();

builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<SmsService>();
builder.Services.AddSingleton<RedisHelper>();
builder.Services.AddTransient<SmsTextMagicService>();

// Configure Redis
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");

if (string.IsNullOrEmpty(redisConnectionString))
{
    Console.WriteLine("Redis connection string is not configured.");
    throw new InvalidOperationException("Redis connection string is not configured.");
}

ConfigurationOptions configurationOptions;
try
{
    configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    configurationOptions.AbortOnConnectFail = false;
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to parse Redis connection string: {ex.Message}");
    throw;
}

IConnectionMultiplexer redis;
try
{
    redis = ConnectionMultiplexer.Connect(configurationOptions);
    Console.WriteLine("Successfully connected to Redis.");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
    throw;
}

builder.Services.AddSingleton(redis);

// Configure Hangfire to use In-Memory storage
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

// Add JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // If the Authorization header is missing or empty, use the refresh token
            if (string.IsNullOrEmpty(token) && context.Request.Cookies.ContainsKey("refreshToken"))
            {
                token = context.Request.Cookies["refreshToken"];
                context.Request.Headers.Append("Token-Type", "Refresh");
            }
            else
            {
                context.Request.Headers.Append("Token-Type", "Access");
            }

            context.Token = token;

            return Task.CompletedTask;
        }
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOnly", builder =>
    {
        builder.WithOrigins("http://localhost:3000", 
                "http://18.159.214.207", 
                "http://plus-appointments-alb-1330428496.eu-central-1.elb.amazonaws.com",
                "https://plus-appointment.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Use CORS
app.UseCors("AllowFrontendOnly");

app.UseAuthentication();
app.UseRoleMiddleware();
app.UseAuthorization();

// Use Hangfire dashboard
app.UseHangfireDashboard("/api/hangfire", new DashboardOptions
{
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});


// Map the SignalR hub
app.MapHub<AppointmentHub>("/appointmentHub");


app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();

// Helper function to ensure the Logs directory is created
void EnsureLogsDirectory()
{
    var logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
    }
}

public partial class Program { } // This is needed for the EF Core CLI tools to function properly
