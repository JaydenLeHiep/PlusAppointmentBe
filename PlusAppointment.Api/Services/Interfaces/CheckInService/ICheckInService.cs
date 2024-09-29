using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.CheckInService;

public interface ICheckInService
{
    Task<IEnumerable<CheckIn?>> GetAllCheckInsAsync();
    Task<CheckIn?> GetCheckInByIdAsync(int id);
    Task<IEnumerable<CheckIn?>> GetCheckInsByBusinessIdAsync(int businessId);
    Task AddCheckInAsync(CheckIn? checkIn);
    Task UpdateCheckInAsync(int checkInId, CheckIn? checkIn);
    Task DeleteCheckInAsync(int checkInId);
}