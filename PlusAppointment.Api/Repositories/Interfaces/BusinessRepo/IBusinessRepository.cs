using PlusAppointment.Models.Classes;

namespace WebApplication1.Repositories.Interfaces.BusinessRepo;

public interface IBusinessRepository
{
    Task<IEnumerable<Business?>> GetAllAsync();
    Task<Business?> GetByIdAsync(int id);
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task DeleteAsync(int id);
    Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId);
    Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId);
}