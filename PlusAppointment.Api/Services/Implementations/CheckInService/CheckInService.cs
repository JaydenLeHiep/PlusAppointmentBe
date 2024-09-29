using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Services.Interfaces.CheckInService;

namespace PlusAppointment.Services.Implementations.CheckInService;

public class CheckInService : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ICustomerRepository _customerRepository;

    public CheckInService(ICheckInRepository checkInRepository, ICustomerRepository customerRepository)
    {
        _checkInRepository = checkInRepository;
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CheckIn?>> GetAllCheckInsAsync()
    {
        return await _checkInRepository.GetAllCheckInsAsync();
    }

    public async Task<CheckIn?> GetCheckInByIdAsync(int id)
    {
        var checkIn = await _checkInRepository.GetCheckInByIdAsync(id);
        if (checkIn == null)
        {
            return null;
        }

        return checkIn;
    }

    public async Task<IEnumerable<CheckIn?>> GetCheckInsByBusinessIdAsync(int businessId)
    {
        return await _checkInRepository.GetCheckInsByBusinessIdAsync(businessId);
    }

    public async Task AddCheckInAsync(CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn), "CheckIn cannot be null.");
        }

        // Check if the customer exists
        var customer = await _customerRepository.GetCustomerByIdAsync(checkIn.CustomerId);
        // Check if the customer exists


        if (customer == null)
        {
            throw new KeyNotFoundException("Customer with the provided email or phone does not exist.");
        }

        // Add the check-in record
        await _checkInRepository.AddCheckInAsync(checkIn);
    }

    public async Task UpdateCheckInAsync(int checkInId, CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn), "CheckIn cannot be null.");
        }

        var existingCheckIn = await _checkInRepository.GetCheckInByIdAsync(checkInId);
        if (existingCheckIn == null)
        {
            throw new KeyNotFoundException("CheckIn not found.");
        }

        // Update fields if provided
        existingCheckIn.CustomerId = checkIn.CustomerId;
        existingCheckIn.BusinessId = checkIn.BusinessId;
        existingCheckIn.CheckInTime = checkIn.CheckInTime;
        existingCheckIn.CheckInType = checkIn.CheckInType;

        await _checkInRepository.UpdateCheckInAsync(existingCheckIn);
    }

    public async Task DeleteCheckInAsync(int checkInId)
    {
        await _checkInRepository.DeleteCheckInAsync(checkInId);
    }
}