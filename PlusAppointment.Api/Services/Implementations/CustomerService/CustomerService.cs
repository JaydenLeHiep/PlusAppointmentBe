using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Services.Interfaces.CustomerService;

namespace PlusAppointment.Services.Implementations.CustomerService;

public class CustomerService: ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    
    public async Task<IEnumerable<Customer?>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllCustomersAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return null;
        }

        return customer;
    }
    
    public async Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone)
    {
        var customer = await _customerRepository.GetCustomerByEmailOrPhoneAsync(emailOrPhone);
        if (customer == null)
        {
            return null;
        }

        return customer;
    }

    public async Task AddCustomerAsync(CustomerDto customerDto)
    {
        if (customerDto == null)
        {
            throw new ArgumentNullException(nameof(customerDto), "CustomerDto cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(customerDto.Name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(customerDto.Name));
        }

        // if (string.IsNullOrWhiteSpace(customerDto.Email))
        // {
        //     throw new ArgumentException("Email cannot be null or empty.", nameof(customerDto.Email));
        // }
        //
        // if (string.IsNullOrWhiteSpace(customerDto.Phone))
        // {
        //     throw new ArgumentException("Phone cannot be null or empty.", nameof(customerDto.Phone));
        // }
        //
        if (!await _customerRepository.IsEmailUniqueAsync(customerDto.Email))
        {
            throw new ArgumentException("Email is already in use.");
        }
        
        if (!await _customerRepository.IsPhoneUniqueAsync(customerDto.Phone))
        {
            throw new ArgumentException("Phone is already in use.");
        }

        var customer = new Customer
        {
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            BusinessId = customerDto.BusinessId
            // Assign other properties as necessary
        };

        await _customerRepository.AddCustomerAsync(customer);
    }

    public async Task UpdateCustomerAsync(int id, CustomerDto customerDto)
    {
        if (customerDto == null)
        {
            throw new ArgumentNullException(nameof(customerDto), "CustomerDto cannot be null.");
        }

        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        // Update only the fields that are provided in the DTO
        if (!string.IsNullOrWhiteSpace(customerDto.Email) && customerDto.Email != existingCustomer.Email)
        {
            if (!await _customerRepository.IsEmailUniqueAsync(customerDto.Email))
            {
                throw new ArgumentException("Email is already in use.");
            }
            existingCustomer.Email = customerDto.Email;
        }

        if (!string.IsNullOrWhiteSpace(customerDto.Phone) && customerDto.Phone != existingCustomer.Phone)
        {
            if (!await _customerRepository.IsPhoneUniqueAsync(customerDto.Phone))
            {
                throw new ArgumentException("Phone is already in use.");
            }
            existingCustomer.Phone = customerDto.Phone;
        }

        if (!string.IsNullOrWhiteSpace(customerDto.Name))
        {
            existingCustomer.Name = customerDto.Name;
        }

        // Update other properties as necessary

        await _customerRepository.UpdateCustomerAsync(existingCustomer);
    }
    
    public async Task DeleteCustomerAsync(int id)
    {
        await _customerRepository.DeleteCustomerAsync(id);
    }
    
    public async Task<IEnumerable<Customer>> SearchCustomersByNameOrPhoneAsync(string searchTerm)
    {
        var customers = await _customerRepository.SearchCustomersByNameOrPhoneAsync(searchTerm);
        return customers.Where(c => c != null)!; // Filter out null values
    }
    
    public async Task<IEnumerable<AppointmentHistoryDto>> GetCustomerAppointmentsAsync(int customerId)
    {
        return await _customerRepository.GetAppointmentsByCustomerIdAsync(customerId);
    }
    
    public async Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone)
    {
        return await _customerRepository.GetCustomerByNameOrPhoneAsync(nameOrPhone);
    }
}