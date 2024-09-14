using Microsoft.Extensions.DependencyInjection;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Tests.Factories;

namespace PlusAppointment.Tests.Staff.StaffRepoTests;

public class StaffRepoWriteIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public StaffRepoWriteIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }
    [Fact]
    public async Task AddListStaffsAsync_AddsStaffsSuccessfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        var staffList = new List<Models.Classes.Staff>
        {
            new Models.Classes.Staff { Name = "New Staff 3", BusinessId = 1 , Email = "", Phone = ""},
            new Models.Classes.Staff { Name = "New Staff 4", BusinessId = 1 , Email = "", Phone = ""}
        };

        // Act
        await staffRepository.AddListStaffsAsync(staffList);

        // Assert: Verify that the staff members were added successfully
        var addedStaff = await staffRepository.GetAllByBusinessIdAsync(1);
        Assert.NotNull(addedStaff);
        Assert.Equal(4, addedStaff.Count()); // Since 2 staff members were seeded, total should be 4 now
    }
    
    [Fact]
    public async Task AddListStaffsAsync_ThrowsException_WhenStaffsIsNull()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        // Act & Assert: Verify that an Exception is thrown when staff list is null
        var exception = await Assert.ThrowsAsync<Exception>(() => staffRepository.AddListStaffsAsync(null));

        // Assert the exception message
        Assert.Equal("Staffs collection cannot be null or empty", exception.Message);
    }
    
    [Fact]
    public async Task AddListStaffsAsync_ThrowsException_WhenBusinessNotFound()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        var staffList = new List<Models.Classes.Staff>
        {
            new Models.Classes.Staff { Name = "Staff for non-existing business", BusinessId = 999 , Email = "", Phone = ""} // BusinessId 999 doesn't exist
        };

        // Act & Assert: Verify that an exception is thrown when the business is not found
        var exception = await Assert.ThrowsAsync<Exception>(() => staffRepository.AddListStaffsAsync(staffList));
        Assert.Equal("Business not found", exception.Message);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesStaffSuccessfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        // Seed and retrieve an existing staff
        var staff = new Models.Classes.Staff { StaffId = 1, Name = "Staff Member 1", Email = "oldemail@example.com", Phone = "1234567890", Password = "oldpassword", BusinessId = 1 };
            
        // Update staff data
        staff.Name = "Updated Staff Member";
        staff.Email = "newemail@example.com";
        staff.Phone = "0987654321";
        staff.Password = "newpassword";
            
        // Act
        await staffRepository.UpdateAsync(staff);

        // Assert: Fetch the updated staff from the database and validate the changes
        var updatedStaff = await staffRepository.GetByIdAsync(1); // Assuming GetByIdAsync exists
            
        Assert.NotNull(updatedStaff);
        Assert.Equal("Updated Staff Member", updatedStaff.Name);
        Assert.Equal("newemail@example.com", updatedStaff.Email);
        Assert.Equal("0987654321", updatedStaff.Phone);
        Assert.Equal("newpassword", updatedStaff.Password);
    }
    [Fact]
    public async Task DeleteAsync_DeletesStaffSuccessfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        // Seed an existing staff (either in your seeding logic or manually)
        var existingStaff = new Models.Classes.Staff { StaffId = 2, BusinessId = 1, Name = "Staff Member 2" };
        Assert.NotNull(existingStaff); 

        // Act: Delete the staff member
        await staffRepository.DeleteAsync(existingStaff.BusinessId, existingStaff.StaffId);

        // Assert: Try to fetch the staff from the database and ensure it throws an exception
        var exception = await Assert.ThrowsAsync<Exception>(() => staffRepository.GetByIdAsync(existingStaff.StaffId));
    
        // Verify the message in the exception is the one we expect
        Assert.Equal($"Staff with ID {existingStaff.StaffId} not found", exception.Message);
    }
    
    
}