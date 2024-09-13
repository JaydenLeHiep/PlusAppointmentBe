using Microsoft.Extensions.Configuration;
using Moq;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Implementations.StaffService;
using PlusAppointment.Utils.Hash;


namespace PlusAppointment.Tests.Staff.StaffServiceTest;

public class StaffServiceReadTest
{
    private readonly Mock<IStaffRepository> _mockStaffRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHashUtility> _mockHashUtility;
    private readonly StaffService _staffService;

    public StaffServiceReadTest()
    {
        // Initialize the mock repository
        _mockStaffRepository = new Mock<IStaffRepository>();

        // Initialize the mock configuration
        _mockConfiguration = new Mock<IConfiguration>();

        _mockHashUtility = new Mock<IHashUtility>();

        // Inject the mocks into the service
        _staffService = new StaffService(_mockStaffRepository.Object, _mockConfiguration.Object, _mockHashUtility.Object);
    }

    [Fact]
    public async Task GetAllStaffsAsync_ReturnsStaffList()
    {
        // Arrange: Set up mock repository to return a list of staff
        var staffList = new List<Models.Classes.Staff>
        {
            new Models.Classes.Staff { StaffId = 1, Name = "Jane Doe" }, // Fix: Changed Id to StaffId
            new Models.Classes.Staff { StaffId = 2, Name = "John Smith" } // Fix: Changed Id to StaffId
        };

        _mockStaffRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(staffList);

        // Act: Call the service method
        var result = await _staffService.GetAllStaffsAsync();

        // Assert: Check that the result is the expected list of staff
        Assert.NotNull(result);
        var enumerable = result as Models.Classes.Staff[] ?? result.ToArray();
        Assert.Equal(2, enumerable.Count());
        Assert.Equal("Jane Doe", enumerable.First().Name);
    }


    [Fact]
    public async Task GetStaffIdAsync_ReturnsStaff_WhenIdIsValid()
    {
        // Arrange: Set up mock repository to return a specific staff
        var staff = new Models.Classes.Staff { StaffId = 1, Name = "Jane Doe" };

        _mockStaffRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(staff);

        // Act: Call the service method
        var result = await _staffService.GetStaffIdAsync(1);

        // Assert: Check that the result is the expected staff
        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.Name);
    }



    [Fact]
    public async Task GetAllStaffByBusinessIdAsync_ReturnsStaffList_WhenBusinessIdIsValid()
    {
        // Arrange: Set up mock repository to return a list of staff for a specific business ID
        var staffList = new List<Models.Classes.Staff?>
        {
            new Models.Classes.Staff { StaffId = 1, Name = "Jane Doe", BusinessId = 1 },
            new Models.Classes.Staff { StaffId = 2, Name = "John Smith", BusinessId = 1 }
        };

        // Mock repository to return the staff list when the business ID is 1
        _mockStaffRepository.Setup(repo => repo.GetAllByBusinessIdAsync(1)).ReturnsAsync(staffList);
        // Act: Call the service method with business ID 1
        var result = await _staffService.GetAllStaffByBusinessIdAsync(1);

        // Assert: Check that the result is the expected list of staff for business ID 1
        Assert.NotNull(result);
        var enumerable = result as Models.Classes.Staff?[] ?? result.ToArray();
        Assert.Equal(2, enumerable.Count());
        Assert.Equal("Jane Doe", enumerable.First()?.Name);
    }

    [Fact]
    public async Task GetAllStaffByBusinessIdAsync_ReturnsEmptyList_WhenNoStaffForBusinessId()
    {
        // Arrange: Set up mock repository to return an empty list for a business with no staff
        var emptyStaffList = new List<Models.Classes.Staff?>();

        // Mock repository to return an empty list when the business ID is 999
        _mockStaffRepository.Setup(repo => repo.GetAllByBusinessIdAsync(999))
            .ReturnsAsync(emptyStaffList);

        // Act: Call the service method with business ID 999
        var result = await _staffService.GetAllStaffByBusinessIdAsync(999);

        // Assert: Check that the result is an empty list
        Assert.NotNull(result);
        Assert.Empty(result); // Verify that the list is empty
    }
    [Fact]
    public async Task GetAllStaffByBusinessIdAsync_ReturnsEmptyList_WhenBusinessIdHasNoStaff()
    {
        // Arrange: Mock repository to return an empty list
        _mockStaffRepository.Setup(repo => repo.GetAllByBusinessIdAsync(1))
            .ReturnsAsync(new List<Models.Classes.Staff?>());

        // Act: Call the service method
        var result = await _staffService.GetAllStaffByBusinessIdAsync(1);

        // Assert: Check that the result is an empty list
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    [Fact]
    public async Task GetAllStaffByBusinessIdAsync_ThrowsException_WhenRepositoryFails()
    {
        // Arrange: Mock repository to throw an exception
        _mockStaffRepository.Setup(repo => repo.GetAllByBusinessIdAsync(1))
            .ThrowsAsync(new Exception("Database failure"));

        // Act & Assert: Ensure an exception is thrown
        await Assert.ThrowsAsync<Exception>(() => _staffService.GetAllStaffByBusinessIdAsync(1));
    }

}