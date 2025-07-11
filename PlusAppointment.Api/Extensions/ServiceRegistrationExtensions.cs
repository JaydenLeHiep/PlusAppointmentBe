using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Implementation.BusinessRepo;
using PlusAppointment.Repositories.Implementation.CalculateMoneyRepo;
using PlusAppointment.Repositories.Implementation.CheckInRepo;
using PlusAppointment.Repositories.Implementation.CustomerRepo;
using PlusAppointment.Repositories.Implementation.DiscountCodeRepo;
using PlusAppointment.Repositories.Implementation.DiscountTierRepo;
using PlusAppointment.Repositories.Implementation.EmailContentRepo;
using PlusAppointment.Repositories.Implementation.EmailUsageRepo;
using PlusAppointment.Repositories.Implementation.NotAvailableDateRepository;
using PlusAppointment.Repositories.Implementation.NotAvailableTimeRepo;
using PlusAppointment.Repositories.Implementation.NotificationRepo;
using PlusAppointment.Repositories.Implementation.OpeningHoursRepository;
using PlusAppointment.Repositories.Implementation.ServiceCategoryRepo;
using PlusAppointment.Repositories.Implementation.ServicesRepo;
using PlusAppointment.Repositories.Implementation.ShopPicturesRepo;
using PlusAppointment.Repositories.Implementation.StaffRepo;
using PlusAppointment.Repositories.Implementation.UserRepo;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;
using PlusAppointment.Repositories.Interfaces.DiscountTierRepo;
using PlusAppointment.Repositories.Interfaces.EmailContentRepo;
using PlusAppointment.Repositories.Interfaces.EmailUsageRepo;
using PlusAppointment.Repositories.Interfaces.IOpeningHoursRepository;
using PlusAppointment.Repositories.Interfaces.NotAvailableDateRepository;
using PlusAppointment.Repositories.Interfaces.NotAvailableTimeRepo;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Repositories.Interfaces.UserRepo;
using PlusAppointment.Services.Implementations.AppointmentService;
using PlusAppointment.Services.Implementations.BusinessService;
using PlusAppointment.Services.Implementations.CalculateMoneyService;
using PlusAppointment.Services.Implementations.CheckInService;
using PlusAppointment.Services.Implementations.CustomerService;
using PlusAppointment.Services.Implementations.DiscountCodeService;
using PlusAppointment.Services.Implementations.DiscountTierService;
using PlusAppointment.Services.Implementations.EmailContentService;
using PlusAppointment.Services.Implementations.EmailSendingService;
using PlusAppointment.Services.Implementations.EmailUsageService;
using PlusAppointment.Services.Implementations.NotAvailableDate;
using PlusAppointment.Services.Implementations.NotAvailableTimeService;
using PlusAppointment.Services.Implementations.NotificationService;
using PlusAppointment.Services.Implementations.OpeningHoursService;
using PlusAppointment.Services.Implementations.ServiceCategoryService;
using PlusAppointment.Services.Implementations.ServicesService;
using PlusAppointment.Services.Implementations.ShopPictureService;
using PlusAppointment.Services.Implementations.StaffService;
using PlusAppointment.Services.Implementations.UserService;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Services.Interfaces.BusinessService;
using PlusAppointment.Services.Interfaces.CalculateMoneyService;
using PlusAppointment.Services.Interfaces.CheckInService;
using PlusAppointment.Services.Interfaces.CustomerService;
using PlusAppointment.Services.Interfaces.DiscountCodeService;
using PlusAppointment.Services.Interfaces.DiscountTierService;
using PlusAppointment.Services.Interfaces.EmailContentService;
using PlusAppointment.Services.Interfaces.EmailSendingService;
using PlusAppointment.Services.Interfaces.EmailUsageService;
using PlusAppointment.Services.Interfaces.IOpeningHoursService;
using PlusAppointment.Services.Interfaces.NotAvailableDateService;
using PlusAppointment.Services.Interfaces.NotAvailableTimeService;
using PlusAppointment.Services.Interfaces.NotificationService;
using PlusAppointment.Services.Interfaces.ServiceCategoryService;
using PlusAppointment.Services.Interfaces.ServicesService;
using PlusAppointment.Services.Interfaces.ShopPictureService;
using PlusAppointment.Services.Interfaces.StaffService;
using PlusAppointment.Services.Interfaces.UserService;
using PlusAppointment.Utils.Errors;
using PlusAppointment.Utils.Hash;
using PlusAppointment.Utils.Mapping;

namespace PlusAppointment.Extensions;

public static class ServiceRegistrationExtensions
{
    public static void RegisterCoreServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers(options =>
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
        
        services.AddAutoMapper(typeof(MappingProfile));
        
        services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 102400; // Set to 100KB
        });
        
        services.AddHttpClient();
    }

    public static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IServicesRepository, ServicesRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IAppointmentReadRepository, AppointmentReadRepository>();
        services.AddScoped<IAppointmentWriteRepository, AppointmentWriteRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICalculateMoneyRepo, CalculateMoneyRepo>();
        services.AddScoped<IServiceCategoryRepo, ServiceCategoryRepo>();
        services.AddScoped<INotAvailableDateRepository, NotAvailableDateRepository>();
        services.AddScoped<IEmailUsageRepo, EmailUsageRepo>();
        services.AddScoped<INotAvailableTimeRepository, NotAvailableTimeRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IOpeningHoursRepository, OpeningHoursRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IEmailContentRepo, EmailContentRepo>();
        services.AddScoped<IDiscountTierRepository, DiscountTierRepository>();
        services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
        services.AddScoped<IShopPictureRepository, ShopPictureRepository>();
    }
    
    public static void RegisterBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<IServicesService, ServicesService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICalculateMoneyService, CalculateMoneyService>();
        services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
        services.AddScoped<INotAvailableDateService, NotAvailableDateService>();
        services.AddScoped<IEmailUsageService, EmailUsageService>();
        services.AddScoped<INotAvailableTimeService, NotAvailableTimeService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IHashUtility, HashUtility>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOpeningHoursService, OpeningHoursService>();
        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<IEmailContentService, EmailContentService>();
        services.AddScoped<IDiscountTierService, DiscountTierService>();
        services.AddScoped<IDiscountCodeService, DiscountCodeService>();
        services.AddScoped<IShopPictureService, ShopPictureService>();
    }

    public static void RegisterAws(this IServiceCollection services, IConfiguration config)
    {
        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(
                config["AWS:AccessKey"],
                config["AWS:SecretKey"]
            ),
            Region = RegionEndpoint.GetBySystemName(config["AWS:Region"])
        };
        services.AddAWSService<Amazon.S3.IAmazonS3>(awsOptions);  // AWS S3 service
        services.AddSingleton<S3Service>();
    }

    public static void RegisterHangfire(this IServiceCollection services, IConfiguration config)
    {
        services.AddHangfire(h =>
            h.UsePostgreSqlStorage(config.GetConnectionString("DefaultConnection"), new PostgreSqlStorageOptions
            {
                InvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15)
            }));
        
        services.AddHangfireServer();
    }

    public static void RegisterJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var key = Encoding.ASCII.GetBytes(config["Jwt:Key"] ?? string.Empty);
        services.AddAuthentication(options =>
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
                        context.HandleResponse();
                        
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var responsePayload = new
                        {
                            error = "Unauthorized",
                            message = "You are not authorized to access this resource."
                        };
                        
                        var json = JsonSerializer.Serialize(responsePayload);

                        await context.Response.WriteAsync(json);
                    }
                };
            });
    }

    public static void RegisterCors(this IServiceCollection services)
    {
        services.AddCors(options =>
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
    }
    
    
}
