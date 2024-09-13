using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Enums; // Your DbContext namespace

namespace PlusAppointment.Tests.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Load the appsettings.Test.json file
                config.AddJsonFile("appsettings.Test.json");
            });

            builder.ConfigureServices((context, services) =>
            {
                // Remove the real ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Use the connection string from the configuration
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                // Add the PostgreSQL test database
                services.AddDbContextFactory<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });


                // Ensure the database is created and seeded with data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                // Seed the test data
                SeedTestData(db);
            });
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            // Reset the tables by truncating them
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE users, businesses, staffs RESTART IDENTITY CASCADE;");
            
            // Seed a User entity
            var user = new User(
                username: "test_user",
                password: "hashedpassword",
                email: "testuser@example.com",
                phone: "1234567890",
                role: Role.Owner
            );

            context.Users.Add(user);
            context.SaveChanges();

            // Create and seed a Business entity
            var business = new Business(
                name: "Test Business", 
                address: "123 Test St", 
                phone: "1234567890", 
                email: "testbusiness@example.com", 
                userID: user.UserId
            );

            context.Businesses.Add(business);
            context.SaveChanges();

            // Seed Staff entities
            context.Staffs.AddRange(
                new Models.Classes.Staff { BusinessId = business.BusinessId, Name = "Staff Member 1", Email = "", Phone = "" },
                new Models.Classes.Staff { BusinessId = business.BusinessId, Name = "Staff Member 2", Email = "", Phone = "" }
            );

            context.SaveChanges();
        }
    }
}
