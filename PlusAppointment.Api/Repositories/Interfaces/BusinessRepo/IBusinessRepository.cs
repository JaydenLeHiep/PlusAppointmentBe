using PlusAppointment.Models.Classes;
using PlusAppointment.Models.Classes.Business;

namespace PlusAppointment.Repositories.Interfaces.BusinessRepo;

public interface IBusinessRepository
{
    Task<IEnumerable<Business?>> GetAllAsync();
    Task<Business?> GetByIdAsync(int id);
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task DeleteAsync(int id);
    Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId);
    Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId);
    
    Task<IEnumerable<Business?>> GetAllByUserIdAsync(int userId);
    Task<Business?> GetByNameAsync(string businessName);
    Task<decimal?> GetBirthdayDiscountPercentageAsync(int businessId);
}