using Microsoft.Extensions.Configuration;
using Moq;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Implementations.StaffService;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Utils.Hash;

namespace PlusAppointment.Tests.Staff.StaffServiceTest;

public class StaffServiceWriteTest
{
    private readonly Mock<IStaffRepository> _mockStaffRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHashUtility> _mockHashUtility;
    private readonly StaffService _staffService;

    public StaffServiceWriteTest()
    {
        // Initialize the mock repository
        _mockStaffRepository = new Mock<IStaffRepository>();

        // Initialize the mock configuration
        _mockConfiguration = new Mock<IConfiguration>();

        _mockHashUtility = new Mock<IHashUtility>();

        // Inject the mocks into the service
        _staffService =
            new StaffService(_mockStaffRepository.Object, _mockConfiguration.Object, _mockHashUtility.Object);
    }

    [Fact]
    public async Task AddStaffAsync_AddsStaff_WhenStaffDtoIsValid()
    {
        var staffDto = new StaffDto
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Phone = "1234567890",
            Password = "password123",
            BusinessId = 1
        };
        // Act: Call the AddStaffAsync method
        await _staffService.AddStaffAsync(staffDto);

        // Assert: Verify that the repository's AddStaffAsync method was called exactly once
        _mockStaffRepository.Verify(repo => repo.AddStaffAsync(It.IsAny<Models.Classes.Staff>(), staffDto.BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task AddStaffAsync_ThrowsArgumentNullException_WhenStaffDtoIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _staffService.AddStaffAsync(null));
    }

    [Fact]
    public async Task AddStaffAsync_ThrowsArgumentException_WhenPasswordIsEmpty()
    {
        var staffDto = new StaffDto
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Phone = "1234567890",
            Password = "",
            BusinessId = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _staffService.AddStaffAsync(staffDto));
    }

    [Fact]
    public async Task AddStaffAsync_CreatesStaffFromDto()
    {
        var staffDto = new StaffDto
        {
            Name = "John Smith",
            Email = "john@example.com",
            Phone = "9876543210",
            Password = "password456",
            BusinessId = 2
        };

        await _staffService.AddStaffAsync(staffDto);

        _mockStaffRepository.Verify(repo => repo.AddStaffAsync(It.Is<Models.Classes.Staff>(staff =>
            staff.Name == "John Smith" &&
            staff.Email == "john@example.com" &&
            staff.Phone == "9876543210" &&
            staff.BusinessId == 2
        ), staffDto.BusinessId), Times.Once);
    }

    [Fact]
    public async Task AddStaffAsync_StoresHashedPassword_WhenPasswordIsHashed()
    {
        // Arrange: Create a StaffDto with a plaintext password
        var staffDto = new StaffDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "1234567890",
            Password = "plaintextpassword123", // Plaintext password
            BusinessId = 1
        };

        // Mock the IHashUtility to return a hashed password
        var mockHashUtility = new Mock<IHashUtility>();
        mockHashUtility.Setup(h => h.HashPassword(staffDto.Password))
            .Returns("hashedpassword123"); // Mock hashed password

        // Capture the actual password passed to the repository for debugging purposes
        string actualPassword = null;
        _mockStaffRepository.Setup(repo => repo.AddStaffAsync(It.IsAny<Models.Classes.Staff>(), It.IsAny<int>()))
            .Callback<Models.Classes.Staff, int>((staff, businessId) =>
            {
                actualPassword = staff.Password; // Capture the actual password
            });

        // Create the service with the mocked IHashUtility
        var staffService = new StaffService(_mockStaffRepository.Object, new Mock<IConfiguration>().Object, mockHashUtility.Object);

        // Act: Call AddStaffAsync
        await staffService.AddStaffAsync(staffDto);

        // Assert: Verify that AddStaffAsync was called exactly once
        _mockStaffRepository.Verify(repo => repo.AddStaffAsync(It.IsAny<Models.Classes.Staff>(), staffDto.BusinessId), Times.Once, "AddStaffAsync should be called exactly once.");

        // Assert: Verify that the password was hashed correctly before being passed to the repository
        Assert.NotNull(actualPassword); // Ensure that the password was actually passed
        Assert.Equal("hashedpassword123", actualPassword); // Ensure that the hashed password is passed
    }

    [Fact]
    public async Task DeleteStaffAsync_CallsRepositoryDelete_WithCorrectParameters()
    {
        // Arrange: Set up businessId and staffId
        int businessId = 1;
        int staffId = 123;

        // Mock the repository
        var mockStaffRepository = new Mock<IStaffRepository>();

        // Create the service
        var staffService = new StaffService(mockStaffRepository.Object, new Mock<IConfiguration>().Object, new Mock<IHashUtility>().Object);

        // Act: Call DeleteStaffAsync
        await staffService.DeleteStaffAsync(businessId, staffId);

        // Assert: Verify that the repository's DeleteAsync method was called once with the correct parameters
        mockStaffRepository.Verify(repo => repo.DeleteAsync(businessId, staffId), Times.Once, "DeleteAsync should be called once with correct parameters.");
    }

    [Fact]
    public async Task UpdateStaffAsync_ThrowsArgumentException_WhenStaffDtoIsNull()
    {
        // Arrange
        var staffService = new StaffService(_mockStaffRepository.Object, new Mock<IConfiguration>().Object, _mockHashUtility.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => staffService.UpdateStaffAsync(1, 123, null));
    }

    [Fact]
    public async Task UpdateStaffAsync_ThrowsKeyNotFoundException_WhenStaffNotFound()
    {
        // Arrange
        _mockStaffRepository.Setup(repo => repo.GetByBusinessIdServiceIdAsync(1, 123))
            .ReturnsAsync((Models.Classes.Staff)null); // Simulate staff not found

        var staffService = new StaffService(_mockStaffRepository.Object, new Mock<IConfiguration>().Object, _mockHashUtility.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => staffService.UpdateStaffAsync(1, 123, new StaffDto()));
    }
    
    [Fact]
    public async Task UpdateStaffAsync_UpdatesStaffFieldsCorrectly()
    {
        // Arrange
        var existingStaff = new Models.Classes.Staff { Name = "Old Name", Email = "old@example.com", Phone = "12345", Password = "oldpassword" };
        var staffDto = new StaffDto { Name = "New Name", Email = "new@example.com", Phone = "98765", Password = "newpassword" };

        _mockStaffRepository.Setup(repo => repo.GetByBusinessIdServiceIdAsync(1, 123))
            .ReturnsAsync(existingStaff); // Simulate finding the staff

        _mockHashUtility.Setup(h => h.HashPassword("newpassword"))
            .Returns("hashedpassword");

        var staffService = new StaffService(_mockStaffRepository.Object, new Mock<IConfiguration>().Object, _mockHashUtility.Object);

        // Act
        await staffService.UpdateStaffAsync(1, 123, staffDto);

        // Assert: Verify that the staff fields were updated and hashed properly
        Assert.Equal("New Name", existingStaff.Name);
        Assert.Equal("new@example.com", existingStaff.Email);
        Assert.Equal("98765", existingStaff.Phone);
        Assert.Equal("hashedpassword", existingStaff.Password);

        // Verify that UpdateAsync was called with the updated staff
        _mockStaffRepository.Verify(repo => repo.UpdateAsync(existingStaff), Times.Once);
    }
    
    [Fact]
    public async Task UpdateStaffAsync_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var existingStaff = new Models.Classes.Staff { Name = "Old Name", Email = "old@example.com", Phone = "12345", Password = "oldpassword" };
        var staffDto = new StaffDto { Name = "New Name", Email = null, Phone = "98765", Password = null }; // Only Name and Phone are updated

        _mockStaffRepository.Setup(repo => repo.GetByBusinessIdServiceIdAsync(1, 123))
            .ReturnsAsync(existingStaff);

        var staffService = new StaffService(_mockStaffRepository.Object, new Mock<IConfiguration>().Object, _mockHashUtility.Object);

        // Act
        await staffService.UpdateStaffAsync(1, 123, staffDto);

        // Assert: Verify only the provided fields were updated
        Assert.Equal("New Name", existingStaff.Name); // Updated
        Assert.Equal("old@example.com", existingStaff.Email); // Not updated
        Assert.Equal("98765", existingStaff.Phone); // Updated
        Assert.Equal("oldpassword", existingStaff.Password); // Not updated (password null)

        _mockStaffRepository.Verify(repo => repo.UpdateAsync(existingStaff), Times.Once);
    }

    [Fact]
    public async Task UpdateStaffAsync_HashesPasswordBeforeUpdating()
    {
        // Arrange
        var existingStaff = new Models.Classes.Staff { Name = "Old Name", Email = "old@example.com", Phone = "12345", Password = "oldpassword" };
        var staffDto = new StaffDto { Password = "newpassword" }; // Only update the password

        _mockStaffRepository.Setup(repo => repo.GetByBusinessIdServiceIdAsync(1, 123))
            .ReturnsAsync(existingStaff); // Simulate finding the staff

        _mockHashUtility.Setup(h => h.HashPassword("newpassword"))
            .Returns("hashedpassword");

        var staffService = new StaffService(_mockStaffRepository.Object, new Mock<IConfiguration>().Object, _mockHashUtility.Object);

        // Act
        await staffService.UpdateStaffAsync(1, 123, staffDto);

        // Assert: Verify that the password was hashed and updated correctly
        Assert.Equal("hashedpassword", existingStaff.Password);

        _mockStaffRepository.Verify(repo => repo.UpdateAsync(existingStaff), Times.Once);
    }

}