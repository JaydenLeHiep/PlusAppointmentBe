using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;



var builder = WebApplication.CreateBuilder(args);

// logging
// Ensure Logs directory exists
EnsureLogsDirectory();

// Load log4net configuration
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));


// Add services to the container.
builder.Services.AddControllers();

// Configure the DbContext with the connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add other services like repositories and service interfaces here
// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<IUserService, UserService>();

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