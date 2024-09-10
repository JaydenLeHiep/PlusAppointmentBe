using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlusAppointment.Models.Classes; 
using PlusAppointment.Data;
using PlusAppointment.Models.Enums; // Your DbContext namespace

namespace PlusAppointment.Tests.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add the in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql("Host=localhost;Port=5435;Database=plus_appointments_test;Username=hieple;Password=hieple");
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
                password: "hashedpassword", // Ensure this is hashed appropriately in real scenarios
                email: "testuser@example.com",
                phone: "1234567890",
                role: Role.Owner // Assuming you're using Role enums such as Admin, Owner, etc.
            );

            context.Users.Add(user);  // Add the user to the context

            // Save changes to generate the UserId for the business reference
            context.SaveChanges();

            // Create and seed a Business entity
            var business = new Business(
                name: "Test Business", 
                address: "123 Test St", 
                phone: "1234567890", 
                email: "testbusiness@example.com", 
                userID: 1 // Assuming there's a user with ID 1 in the system
            );

            context.Businesses.Add(business);
            
            // Save the Business entity first to ensure BusinessId is generated
            context.SaveChanges();
            // Seed test data
            context.Staffs.AddRange(
                new Models.Classes.Staff { StaffId = 1, BusinessId = business.BusinessId, Name = "Staff Member 1", Email = "", Phone = ""},
                new Models.Classes.Staff { StaffId = 2, BusinessId = business.BusinessId, Name = "Staff Member 2" , Email = "", Phone = ""}
            );

            context.SaveChanges();
        }
    }
}