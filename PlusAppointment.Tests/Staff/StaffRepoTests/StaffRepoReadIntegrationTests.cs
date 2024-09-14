
using Microsoft.Extensions.DependencyInjection;

using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Tests.Factories;


namespace PlusAppointment.Tests.Staff.StaffRepoTests;

public class StaffRepoReadIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public StaffRepoReadIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllStaffs()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        // Act
        var staffList = await staffRepository.GetAllAsync();

        // Assert
        Assert.NotNull(staffList);
       
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAStaff()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        var staff = await staffRepository.GetByIdAsync(1);
        
        Assert.NotNull(staff); // Check that a staff object is returned
        Assert.IsType<Models.Classes.Staff>(staff); // Ensure the returned object is of type Staff
        
    }
    
    [Fact]
    public async Task GetAllByBusinessIdAsync_ReturnsAllStaffs()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        // Act
        var staffList = await staffRepository.GetAllByBusinessIdAsync(1);

        // Assert
        Assert.NotNull(staffList);
        
    }
    
    [Fact]
    public async Task GetByBusinessIdServiceIdAsync_ReturnsAStaff()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

        var staff = await staffRepository.GetByBusinessIdServiceIdAsync(1, 1);
        
        Assert.NotNull(staff); // Check that a staff object is returned
        Assert.IsType<Models.Classes.Staff>(staff); // Ensure the returned object is of type Staff
       
    }
    
}