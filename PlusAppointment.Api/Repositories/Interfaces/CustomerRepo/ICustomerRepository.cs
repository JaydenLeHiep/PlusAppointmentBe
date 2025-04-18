using PlusAppointment.Models.Classes;

using PlusAppointment.Models.DTOs.Appointment;

namespace PlusAppointment.Repositories.Interfaces.CustomerRepo;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer?>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    
    Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone);
    Task AddCustomerAsync(Customer? customer);
    Task UpdateCustomerAsync(Customer? customer);
    Task DeleteCustomerAsync(int customerId);
    Task<bool> IsEmailUniqueAsync(int businessId, string email);
    Task<bool> IsPhoneUniqueAsync(int businessId, string phone);
    Task<IEnumerable<Customer?>> SearchCustomersByNameOrPhoneAsync(string searchTerm);
    Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone);
    Task<IEnumerable<AppointmentHistoryDto>> GetAppointmentsByCustomerIdAsync(int customerId);
    Task<IEnumerable<Customer?>> GetCustomersByBusinessIdAsync(int businessId);
    Task<Customer?> GetCustomerByEmailOrPhoneAndBusinessIdAsync(string emailOrPhone, int businessId);
    Task<IEnumerable<Customer?>> GetCustomersWithUpcomingBirthdayAsync(DateTime date);
}