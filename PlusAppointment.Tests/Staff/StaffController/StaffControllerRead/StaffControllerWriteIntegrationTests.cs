using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums; // Adjust namespaces accordingly
using PlusAppointment.Tests.Factories;
using PlusAppointment.Utils.Jwt;

namespace PlusAppointment.Tests.Staff.StaffController.StaffControllerRead;

public class StaffControllerWriteIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public StaffControllerWriteIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task AddStaff_ShouldReturnOk_WhenUserIsAdmin()
    {
        // Arrange: Create a user with the Admin role and set the Authorization header
        var adminUser = new User("admin_user", "hashedpassword", "admin@example.com", "1234567890", Role.Admin);
        SetAuthorizationHeader(adminUser);

        int businessId = 1; // Set businessId for which staff is being added
        var staffDto = new StaffDto
        {
            Name = "John",
            Email = "johndoe@example.com",
            Phone = "1234567890",
            Password = "123",
            BusinessId = businessId
        };

        // Convert staffDto to JSON for the request body
        var content = new StringContent(JsonConvert.SerializeObject(staffDto), System.Text.Encoding.UTF8, "application/json");

        // Act: Send a POST request to add the staff
        var response = await _client.PostAsync($"/api/staff/business_id={businessId}/add", content); // Updated URL

        // Assert: Ensure the response is OK (200) and the staff was added
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseString);

        Assert.Equal("Staff added successfully", (string)result.message);
    }


    [Fact]
    public async Task AddStaff_ShouldReturnNotFound_WhenUserIsNotAdminOrOwner()
    {
        // Arrange: Create a user with a non-admin/owner role (e.g., Staff)
        var staffUser = new User("staff_user", "hashedpassword", "staff@example.com", "1234567890", Role.Staff);
        SetAuthorizationHeader(staffUser);

        int businessId = 1; // Set businessId for which staff is being added
        var staffDto = new StaffDto
        {
            Name = "John",
            Email = "johndoe@example.com",
            Phone = "1234567890",
            BusinessId = businessId
        };

        // Convert staffDto to JSON for the request body
        var content = new StringContent(JsonConvert.SerializeObject(staffDto), System.Text.Encoding.UTF8,
            "application/json");

        // Act: Send a POST request to add the staff
        var response = await _client.PostAsync($"/api/staff/business_id={businessId}/add", content);

        // Assert: Ensure the response is NotFound (404)
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateStaff_ShouldReturnOk_WhenDataIsValid()
    {
        // Arrange: Set the Authorization header (role does not matter here)
        var user = new User("test_user", "hashedpassword", "test@example.com", "1234567890", Role.Admin); // Role is irrelevant
        SetAuthorizationHeader(user);

        int businessId = 1; // This business is already seeded in the in-memory database
        int staffId = 1;    // StaffId is also seeded in the in-memory database

        var staffDto = new StaffDto
        {
            Name = "Updated Staff Name",
            Email = "updatedemail@example.com",
            Phone = "9876543210",
            Password = "updatedpassword",
            BusinessId = businessId
        };

        // Convert staffDto to JSON for the request body
        var content = new StringContent(JsonConvert.SerializeObject(staffDto), System.Text.Encoding.UTF8, "application/json");

        // Act: Send a PUT request to update the staff
        var response = await _client.PutAsync($"/api/staff/business_id={businessId}/staff_id={staffId}", content);


        // Assert: Ensure the response is OK (200) and the staff was updated
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseString);

        Assert.Equal("Staff updated successfully", (string)result.message);

        // // Optionally make a GET request to verify the updated data
        // var getResponse = await _client.GetAsync($"/api/staff/business_id={businessId}/staff_id={staffId}");
        // getResponse.EnsureSuccessStatusCode();
        // var getResponseString = await getResponse.Content.ReadAsStringAsync();
        // var updatedStaff = JsonConvert.DeserializeObject<StaffDto>(getResponseString);
        //
        // // Assert: Ensure the updated staff details are correct
        // Assert.Equal("Updated Staff Name", updatedStaff.Name);
        // Assert.Equal("updatedemail@example.com", updatedStaff.Email);
    }
    
    [Fact]
    public async Task DeleteStaff_ShouldReturnOk_WhenUserIsAdmin()
    {
        // Arrange: Create a user with the Admin role and set the Authorization header
        var adminUser = new User("admin_user", "hashedpassword", "admin@example.com", "1234567890", Role.Admin);
        SetAuthorizationHeader(adminUser);

        int businessId = 1;  // This business is already seeded in the in-memory database
        int staffId = 1;     // StaffId is also seeded in the in-memory database

        // Act: Send a DELETE request to delete the staff
        var response = await _client.DeleteAsync($"/api/staff/business_id={businessId}/staff_id={staffId}");

        // Assert: Ensure the response is OK (200) and the staff was deleted
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseString);

        Assert.Equal("Staff deleted successfully", (string)result.message);
    }


}