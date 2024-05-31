using PlusAppointment.Models.DTOs;
using WebApplication1.Models;
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
    
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllCustomersAsync();
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        return await _customerRepository.GetCustomerByIdAsync(id);
    }

    public async Task AddCustomerAsync(CustomerDto customerDto)
    {
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
        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException("Customer not found.");
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
            CustomerId = id,
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone
            // Assign other properties as necessary
        };

        await _customerRepository.UpdateCustomerAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        await _customerRepository.DeleteCustomerAsync(id);
    }
}