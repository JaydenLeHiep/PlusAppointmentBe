using PlusAppointment.Models.DTOs;

namespace WebApplication1.Services.Interfaces.CustomerService;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> GetCustomerByIdAsync(int id);
    Task AddCustomerAsync(CustomerDto customerDto);
    Task UpdateCustomerAsync(int id, CustomerDto customerDto);
    Task DeleteCustomerAsync(int id);
}