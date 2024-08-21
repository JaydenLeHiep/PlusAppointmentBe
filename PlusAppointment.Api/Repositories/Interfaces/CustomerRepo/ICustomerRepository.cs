using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;

namespace PlusAppointment.Repositories.Interfaces.CustomerRepo;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer?>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    
    Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone);
    Task AddCustomerAsync(Customer? customer);
    Task UpdateCustomerAsync(Customer? customer);
    Task DeleteCustomerAsync(int customerId);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsPhoneUniqueAsync(string phone);
    Task<IEnumerable<Customer?>> SearchCustomersByNameOrPhoneAsync(string searchTerm);
    Task<IEnumerable<AppointmentHistoryDto>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Customer?>> GetCustomersByBusinessIdAsync(int businessId);
}