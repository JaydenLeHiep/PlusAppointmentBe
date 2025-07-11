using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Middleware;
using Hangfire;
using PlusAppointment.CronJobs.DeleteAppointmentsLastMonthJob;
using PlusAppointment.CronJobs.EmailJob;
using PlusAppointment.Extensions;
using PlusAppointment.Utils.CustomAuthorizationFilter;
using PlusAppointment.Utils.Hub;

var builder = WebApplication.CreateBuilder(args);

// Load appsettings configuration
builder.ConfigureAppSettings();

// Register Services
builder.Services.RegisterCoreServices(builder.Configuration);
builder.Services.RegisterRepositories();
builder.Services.RegisterBusinessServices();
builder.Services.RegisterAws(builder.Configuration);
builder.Services.RegisterHangfire(builder.Configuration);
builder.Services.RegisterJwtAuthentication(builder.Configuration);
builder.Services.RegisterCors();

// Load log4net configuration
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Register the DbContext using the factory method
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.UseCors("AllowFrontendOnly");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseRoleMiddleware();
app.UseAuthorization();

// Cron jobs
using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    jobManager.AddOrUpdate<BirthdayEmailJob>(
        "SendBirthdayEmails",
        job => job.ExecuteAsync(),
        Cron.Daily
    );

    jobManager.AddOrUpdate<DeleteAppointmentsLastMonthJob>(
        "DeleteAppointmentsLastMonth",
        job => job.ExecuteAsync(),
        Cron.Monthly
    );
}


// Use Hangfire dashboard
app.UseHangfireDashboard("/api/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllUsersAuthorizationFilter() }
});
// Map the SignalR hub
app.MapHub<AppointmentHub>("/appointmentHub");

app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();

public partial class Program { } // This is needed for the EF Core CLI tools to function properly
