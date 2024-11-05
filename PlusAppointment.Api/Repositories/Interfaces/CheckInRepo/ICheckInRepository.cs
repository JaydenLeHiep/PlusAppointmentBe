
using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Repositories.Interfaces.CheckInRepo;

public interface ICheckInRepository
{
    Task<IEnumerable<CheckIn?>> GetAllCheckInsAsync();
    Task<CheckIn?> GetCheckInByIdAsync(int checkInId);
    Task<IEnumerable<CheckIn?>> GetCheckInsByBusinessIdAsync(int businessId);
    Task AddCheckInAsync(CheckIn? checkIn);
    Task UpdateCheckInAsync(CheckIn? checkIn);
    Task DeleteCheckInAsync(int checkInId);
    Task<bool> HasCheckedInTodayAsync(int businessId, int customerId, DateTime checkInDate);
}