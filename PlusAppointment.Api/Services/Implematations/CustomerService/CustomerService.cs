using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using WebApplication1.Repositories.Interfaces.CustomerRepo;
using WebApplication1.Services.Interfaces.CustomerService;

namespace WebApplication1.Services.Implematations.CustomerService;

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

        if (string.IsNullOrWhiteSpace(customerDto.Email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(customerDto.Email));
        }

        if (string.IsNullOrWhiteSpace(customerDto.Phone))
        {
            throw new ArgumentException("Phone cannot be null or empty.", nameof(customerDto.Phone));
        }

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
            Phone = customerDto.Phone
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
}