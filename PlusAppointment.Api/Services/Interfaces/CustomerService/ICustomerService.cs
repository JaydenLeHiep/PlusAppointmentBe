using PlusAppointment.Models.Classes;

using PlusAppointment.Models.DTOs.Appointment;
using PlusAppointment.Models.DTOs.Customers;

namespace PlusAppointment.Services.Interfaces.CustomerService;
 
 public interface ICustomerService
 {
     Task<IEnumerable<CustomerRetrieveDto?>> GetAllCustomersAsync();
     Task<CustomerRetrieveDto?> GetCustomerByIdAsync(int id);
     
     Task<CustomerRetrieveDto?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone);
     
     Task<IEnumerable<CustomerRetrieveDto?>> GetCustomersByBusinessIdAsync(int businessId);
     Task AddCustomerAsync( int businessId,CustomerDto customerDto);
     Task UpdateCustomerAsync(int customerId, CustomerDto customerDto);
     Task DeleteCustomerAsync(int customerId);
     Task<IEnumerable<Customer>> SearchCustomersByNameOrPhoneAsync(string searchTerm);
     Task<IEnumerable<AppointmentHistoryDto>> GetCustomerAppointmentsAsync(int customerId);

     

     Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone);
     
     Task<Customer?> GetCustomerByEmailOrPhoneAndBusinessIdAsync(string emailOrPhone, int businessId);

 }