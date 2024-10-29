using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.DTOs.Staff;
using PlusAppointment.Models.Enums;
using PlusAppointment.Tests.Factories;
using PlusAppointment.Utils.Jwt;
// Adjust namespaces accordingly

namespace PlusAppointment.Tests.Staff.StaffController;

public class StaffControllerReadIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public StaffControllerReadIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(); // Use the custom factory
        _configuration = factory.Services.GetRequiredService<IConfiguration>(); // Get the configuration for JWT
    }
    
    private void SetAuthorizationHeader(User user)
    {
        var token = JwtUtility.GenerateJwtToken(user, _configuration); // Generate token based on the User object
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }



    [Fact]
    public async Task GetAllStaffByBusinessId_ShouldReturnOk_WhenStaffExists()
    {
        // Arrange
        int businessId = 1; // Assume valid business ID with staff members

        // Act
        var response = await _client.GetAsync($"/api/staff/business_id={businessId}");
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var staff = JsonConvert.DeserializeObject<List<StaffDto>>(responseString);

        // Assert
        Assert.NotNull(staff);
        Assert.True(staff.Count > 0);
    }

    [Fact]
    public async Task GetAllStaffByBusinessId_ShouldReturnNotFound_WhenNoStaffExists()
    {
        // Arrange
        int businessId = 99; // Assume no staff exists for this business ID

        // Act
        var response = await _client.GetAsync($"/api/staff/business_id={businessId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenUserIsAdmin()
    {
        // Arrange: Create a user with Admin role
        var adminUser = new User("admin_user", "hashedpassword", "admin@example.com", "1234567890", Role.Admin);
        SetAuthorizationHeader(adminUser); // Set the Authorization header with the JWT token

        // Act
        var response = await _client.GetAsync("/api/staff");

        // Assert
        response.EnsureSuccessStatusCode();  // Status Code 200-299

        var responseString = await response.Content.ReadAsStringAsync();
        var staffList = JsonConvert.DeserializeObject<List<StaffDto>>(responseString);

        Assert.NotNull(staffList); // Ensure the staff list is returned
    }

    [Fact]
    public async Task GetAll_ShouldReturnNotFound_WhenUserIsNotAdmin()
    {
        // Arrange: Create a user with regular User role (non-admin)
        var regularUser = new User("regular_user", "hashedpassword", "user@example.com", "1234567890", Role.Staff);
        SetAuthorizationHeader(regularUser); // Set the Authorization header with the JWT token

        // Act
        var response = await _client.GetAsync("/api/staff");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}