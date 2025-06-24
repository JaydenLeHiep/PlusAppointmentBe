using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
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

using PlusAppointment.Services.Implementations.AppointmentService;
using PlusAppointment.Services.Implementations.BusinessService;
using PlusAppointment.Services.Implementations.CustomerService;
using PlusAppointment.Services.Implementations.ServicesService;
using PlusAppointment.Services.Implementations.StaffService;
using PlusAppointment.Services.Implementations.UserService;
using PlusAppointment.Utils.Redis;
using PlusAppointment.Utils.SendingSms;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Implementation.CalculateMoneyRepo;
using PlusAppointment.Repositories.Implementation.EmailUsageRepo;
using PlusAppointment.Repositories.Implementation.NotAvailableDateRepository;
using PlusAppointment.Repositories.Implementation.NotificationRepo;
using PlusAppointment.Repositories.Implementation.ServiceCategoryRepo;
using PlusAppointment.Repositories.Implementation.ShopPicturesRepo;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Repositories.Interfaces.EmailUsageRepo;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;
using PlusAppointment.Services.Implementations.CalculateMoneyService;
using PlusAppointment.Services.Implementations.ServiceCategoryService;
using PlusAppointment.Services.Interfaces.CalculateMoneyService;
using PlusAppointment.Services.Interfaces.ServiceCategoryService;
using PlusAppointment.Repositories.Interfaces.NotAvailableDateRepository;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;
using PlusAppointment.Services.Implementations.EmailUsageService;
using PlusAppointment.Services.Implementations.NotAvailableDate;
using PlusAppointment.Services.Implementations.NotificationService;
using PlusAppointment.Services.Implementations.ShopPictureService;
using PlusAppointment.Services.Interfaces.EmailUsageService;
using PlusAppointment.Services.Interfaces.NotAvailableDateService;
using PlusAppointment.Services.Interfaces.NotAvailableTimeService;
using PlusAppointment.Repositories.Interfaces.NotAvailableTimeRepo;
using PlusAppointment.Services.Implementations.NotAvailableTimeService;
using PlusAppointment.Repositories.Implementation.NotAvailableTimeRepo;
using PlusAppointment.Services.Interfaces.NotificationService;
using PlusAppointment.Services.Interfaces.ShopPictureService;

using PlusAppointment.Services.Implementations.OpeningHoursService;
using PlusAppointment.Repositories.Implementation.OpeningHoursRepository;
using PlusAppointment.Services.Interfaces.IOpeningHoursService;
using PlusAppointment.Repositories.Interfaces.IOpeningHoursRepository;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Implementation.CheckInRepo;
using PlusAppointment.Repositories.Implementation.DiscountCodeRepo;
using PlusAppointment.Repositories.Implementation.DiscountTierRepo;
using PlusAppointment.Repositories.Implementation.EmailContentRepo;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;
using PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;
using PlusAppointment.Repositories.Interfaces.DiscountTierRepo;
using PlusAppointment.Repositories.Interfaces.EmailContentRepo;
using PlusAppointment.Services.Implementations.CheckInService;
using PlusAppointment.Services.Implementations.DiscountCodeService;
using PlusAppointment.Services.Implementations.DiscountTierService;
using PlusAppointment.Services.Implementations.EmailContentService;
using PlusAppointment.Services.Implementations.EmailSendingService;
using PlusAppointment.Services.Implementations.GoogleService;
using PlusAppointment.Services.Interfaces.CheckInService;
using PlusAppointment.Services.Interfaces.DiscountCodeService;
using PlusAppointment.Services.Interfaces.DiscountTierService;
using PlusAppointment.Services.Interfaces.EmailContentService;
using PlusAppointment.Services.Interfaces.EmailSendingService;
using PlusAppointment.Services.Interfaces.IGoogleReviewService;
using PlusAppointment.Utils.CustomAuthorizationFilter;
using PlusAppointment.Utils.EmailJob;
using PlusAppointment.Utils.Errors;
using PlusAppointment.Utils.Hash;
using PlusAppointment.Utils.Hub;
using PlusAppointment.Utils.Mapping;
using PlusAppointment.Utils.SQS;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())  // Set base path for configurations
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // Load the base config
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)  // Load environment-specific config
    .AddEnvironmentVariables();  // Override with environment variables

// Bind AppSettings from appsettings.json
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Configure AWS options based on the environment
var awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"]
    ),
    Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
};

// Ensure the Logs directory exists
EnsureLogsDirectory();
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Add services to the container.
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationErrorFilter>(); // Global validation filter
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 102400; // Set to 100KB, adjust based on your object size
}); // Add SignalR services

// Register the DbContext using the factory method
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
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

// Register the new read and write repositories separately
builder.Services.AddScoped<IAppointmentReadRepository, AppointmentReadRepository>();
builder.Services.AddScoped<IAppointmentWriteRepository, AppointmentWriteRepository>();

// Register the AppointmentService as before
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<ICalculateMoneyRepo, CalculateMoneyRepo>();
builder.Services.AddScoped<ICalculateMoneyService, CalculateMoneyService>();

builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
builder.Services.AddScoped<IServiceCategoryRepo, ServiceCategoryRepo>();

builder.Services.AddScoped<INotAvailableDateRepository, NotAvailableDateRepository>();
builder.Services.AddScoped<INotAvailableDateService, NotAvailableDateService>();

builder.Services.AddScoped<IEmailUsageRepo, EmailUsageRepo>();
builder.Services.AddScoped<IEmailUsageService, EmailUsageService>();

builder.Services.AddScoped<INotAvailableTimeRepository, NotAvailableTimeRepository>();
builder.Services.AddScoped<INotAvailableTimeService, NotAvailableTimeService>();

builder.Services.AddScoped<IEmailService, EmailService>(); // Register interface and its implementation


builder.Services.AddScoped<IHashUtility, HashUtility>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register the OpeningHours repository and service
builder.Services.AddScoped<IOpeningHoursRepository, OpeningHoursRepository>();
builder.Services.AddScoped<IOpeningHoursService, OpeningHoursService>();

builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<ICheckInService, CheckInService>();

// email content
builder.Services.AddScoped<IEmailContentRepo, EmailContentRepo>();
builder.Services.AddScoped<IEmailContentService, EmailContentService>();

// discount tier
builder.Services.AddScoped<IDiscountTierRepository, DiscountTierRepository>();
builder.Services.AddScoped<IDiscountTierService, DiscountTierService>();

// discount code
builder.Services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
builder.Services.AddScoped<IDiscountCodeService, DiscountCodeService>();

builder.Services.AddSingleton<SmsService>();
builder.Services.AddSingleton<RedisHelper>();
builder.Services.AddTransient<SmsTextMagicService>();

builder.Services.AddScoped<SqsConsumer>();

builder.Services.AddScoped<IShopPictureRepository, ShopPictureRepository>();
builder.Services.AddScoped<IShopPictureService, ShopPictureService>();

builder.Services.AddHttpClient(); // Enables injecting HttpClient into controllers like GoogleReviewsController

builder.Services.AddAWSService<Amazon.S3.IAmazonS3>(awsOptions);  // AWS S3 service
builder.Services.AddSingleton<S3Service>();
builder.Services.AddScoped<BirthdayEmailJob>();

builder.Services.AddScoped<IGoogleReviewService, GoogleReviewService>();

// mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

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
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new Hangfire.PostgreSql.PostgreSqlStorageOptions
    {
        InvisibilityTimeout = TimeSpan.FromMinutes(5), // Customize options as needed
        QueuePollInterval = TimeSpan.FromSeconds(15)   // Default queue polling interval
    });
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

            // If the Authorization header is missing or empty, try to use the refresh token from cookies.
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
        },
        OnChallenge = async context =>
        {
            // Skip the default behavior.
            context.HandleResponse();

            // Set the response status code and content type.
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            // Create your custom payload.
            var responsePayload = new
            {
                error = "Unauthorized",
                message = "You are not authorized to access this resource."
            };

            // Serialize the payload to JSON.
            var json = JsonSerializer.Serialize(responsePayload);

            // Write the JSON payload to the response.
            await context.Response.WriteAsync(json);
        }
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOnly", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://10.0.2.2:3000",
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
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseRoleMiddleware();
app.UseAuthorization();

// Use Hangfire dashboard
// Use Hangfire dashboard
// Retrieve allowed IPs from appsettings.json before configuring Hangfire dashboard
app.UseHangfireDashboard("/api/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllUsersAuthorizationFilter() }
});

// Schedule the BirthdayEmailJob to run daily
RecurringJob.AddOrUpdate<BirthdayEmailJob>(
    "SendBirthdayEmails", // The job ID
    job => job.ExecuteAsync(), // The method to run
    Cron.Daily // Run the job daily
);
// Trigger the BirthdayEmailJob to run immediately for testing
//BackgroundJob.Enqueue<BirthdayEmailJob>(job => job.ExecuteAsync());

// Schedule the SqsConsumer background job to run periodically
// RecurringJob.AddOrUpdate<SqsConsumer>(
//     consumer => consumer.ProcessEmailQueueAsync(),
//     Cron.Minutely); // This runs the job every minute (you can adjust this interval)
//


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

// Load environment-specific configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())  // Set base path for configurations
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // Load the base config
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)  // Load environment-specific config
    .AddEnvironmentVariables();

// Bind AppSettings from appsettings.json
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

public partial class Program { } // This is needed for the EF Core CLI tools to function properly
