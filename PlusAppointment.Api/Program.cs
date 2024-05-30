using System.Text;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApplication1.Data;
using WebApplication1.Repositories.Implementation.UserRepo;
using WebApplication1.Repositories.Interfaces.UserRepo;
using WebApplication1.Services.Implematations.UserService;
using WebApplication1.Services.Interfaces.UserService;
using WebApplication1.Repositories.Implementation.BusinessRepo;
using WebApplication1.Repositories.Implementation.ServicesRepo;
using WebApplication1.Repositories.Implementation.StaffRepo;
using WebApplication1.Repositories.Interfaces.BusinessRepo;
using WebApplication1.Repositories.Interfaces.ServicesRepo;
using WebApplication1.Repositories.Interfaces.StaffRepo;
using WebApplication1.Services.Implematations.BusinessService;
using WebApplication1.Services.Implematations.ServicesService;
using WebApplication1.Services.Implematations.StaffService;
using WebApplication1.Services.Interfaces.BusinessService;
using WebApplication1.Services.Interfaces.ServicesService;
using WebApplication1.Services.Interfaces.StaffService;

var builder = WebApplication.CreateBuilder(args);

// Ensure the Logs directory exists
EnsureLogsDirectory();
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Add services to the container.
builder.Services.AddControllers();

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

// Add JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOnly", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader();
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
