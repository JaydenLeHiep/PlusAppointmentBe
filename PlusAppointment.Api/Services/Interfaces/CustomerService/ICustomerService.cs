using PlusAppointment.Models.Classes;
 using PlusAppointment.Models.DTOs;
 
 namespace PlusAppointment.Services.Interfaces.CustomerService;
 
 public interface ICustomerService
 {
     Task<IEnumerable<Customer?>> GetAllCustomersAsync();
     Task<Customer?> GetCustomerByIdAsync(int id);
     
     Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone);
     Task AddCustomerAsync(CustomerDto customerDto);
     Task UpdateCustomerAsync(int businessId, int customerId, CustomerDto customerDto);
     Task DeleteCustomerAsync(int businessId,int customerId);
     Task<IEnumerable<Customer>> SearchCustomersByNameOrPhoneAsync(string searchTerm);
     Task<IEnumerable<AppointmentHistoryDto>> GetCustomerAppointmentsAsync(int customerId);
     Task<IEnumerable<Customer?>> GetCustomersByBusinessIdAsync(int businessId);
 }