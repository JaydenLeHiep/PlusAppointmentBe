using PlusAppointment.Models.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces.CustomerRepo;

public interface ICustomerRepository
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> GetCustomerByIdAsync(int customerId);
    Task AddCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int customerId);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsPhoneUniqueAsync(string phone);
}