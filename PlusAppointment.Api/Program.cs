using System.Text;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApplication1.Data;
using WebApplication1.Middleware;

using WebApplication1.Repositories.Implementation.UserRepo;
using WebApplication1.Repositories.Interfaces.UserRepo;
using WebApplication1.Services.Interfaces.UserService;
using WebApplication1.Repositories.Implementation.BusinessRepo;
using WebApplication1.Repositories.Implementation.CustomerRepo;
using WebApplication1.Repositories.Implementation.ServicesRepo;
using WebApplication1.Repositories.Implementation.StaffRepo;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;
using WebApplication1.Repositories.Interfaces.BusinessRepo;
using WebApplication1.Repositories.Interfaces.CustomerRepo;
using WebApplication1.Repositories.Interfaces.ServicesRepo;
using WebApplication1.Repositories.Interfaces.StaffRepo;
using WebApplication1.Services.Interfaces.AppointmentService;
using WebApplication1.Services.Interfaces.BusinessService;
using WebApplication1.Services.Interfaces.CustomerService;
using WebApplication1.Services.Interfaces.ServicesService;
using WebApplication1.Services.Interfaces.StaffService;

using StackExchange.Redis;
using WebApplication1.Repositories.Implementation.AppointmentRepo;
using WebApplication1.Services.Implementations.AppointmentService;
using WebApplication1.Services.Implementations.BusinessService;
using WebApplication1.Services.Implementations.CustomerService;
using WebApplication1.Services.Implementations.ServicesService;
using WebApplication1.Services.Implementations.StaffService;
using WebApplication1.Services.Implementations.UserService;
using WebApplication1.Utils.Redis;

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

builder.Services.AddSingleton<RedisHelper>();

// Configure Redis
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");

if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string is not configured.");
}

var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
configurationOptions.AbortOnConnectFail = false;
var redis = ConnectionMultiplexer.Connect(configurationOptions);

builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

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
        builder.WithOrigins("http://localhost:3000")
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
