using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.BusinessService;

public interface IBusinessService
{
    Task<IEnumerable<Business?>> GetAllBusinessesAsync();
    Task<Business?> GetBusinessByIdAsync(int id);
    Task AddBusinessAsync(Business business);
    Task UpdateBusinessAsync(Business business);
    Task DeleteBusinessAsync(int id);
    Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId);
    Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId);
    
    Task<IEnumerable<Business?>> GetAllBusinessesByUserIdAsync(int userId);
}