using PlusAppointment.Models.Classes;

namespace WebApplication1.Services.Interfaces.BusinessService;

public interface IBusinessService
{
    Task<IEnumerable<Business?>> GetAllBusinessesAsync();
    Task<Business?> GetBusinessByIdAsync(int id);
    Task AddBusinessAsync(Business business);
    Task UpdateBusinessAsync(Business business);
    Task DeleteBusinessAsync(int id);
    Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId);
    Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId);
}