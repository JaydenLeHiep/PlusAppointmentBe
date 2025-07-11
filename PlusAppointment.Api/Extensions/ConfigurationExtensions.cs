using PlusAppointment.Models.Classes;

namespace PlusAppointment.Extensions;

public static class ConfigurationExtensions
{
    public static void ConfigureAppSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    }
}