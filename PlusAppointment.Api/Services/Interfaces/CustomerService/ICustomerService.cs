using PlusAppointment.Models.Classes;
 using PlusAppointment.Models.DTOs;
 
 namespace PlusAppointment.Services.Interfaces.CustomerService;
 
 public interface ICustomerService
 {
     Task<IEnumerable<Customer?>> GetAllCustomersAsync();
     Task<Customer?> GetCustomerByIdAsync(int id);
     
     Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone);
     Task AddCustomerAsync(CustomerDto customerDto);
     Task UpdateCustomerAsync(int id, CustomerDto customerDto);
     Task DeleteCustomerAsync(int id);
     Task<IEnumerable<Customer>> SearchCustomersByNameOrPhoneAsync(string searchTerm);
     Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone);
 }