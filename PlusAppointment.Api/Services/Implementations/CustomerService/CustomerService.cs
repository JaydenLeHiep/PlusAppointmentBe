using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Services.Interfaces.CustomerService;

namespace PlusAppointment.Services.Implementations.CustomerService;

public class CustomerService : ICustomerService
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

    public async Task<Customer?> GetCustomerByEmailOrPhoneAndBusinessIdAsync(string emailOrPhone, int businessId)
    {
        return await _customerRepository.GetCustomerByEmailOrPhoneAndBusinessIdAsync(emailOrPhone, businessId);
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
        // if (!await _customerRepository.IsEmailUniqueAsync(customerDto.Email))
        // {
        //     throw new ArgumentException("Email is already in use.");
        // }
        //
        // if (!await _customerRepository.IsPhoneUniqueAsync(customerDto.Phone))
        // {
        //      throw new ArgumentException("Phone is already in use.");
        // }

        var customer = new Customer
        {
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            BusinessId = customerDto.BusinessId,
            Birthday = customerDto.Birthday, // Set birthday
            WantsPromotion = customerDto.WantsPromotion, // Set promotion preference
            Note = customerDto.Note
        };

        await _customerRepository.AddCustomerAsync(customer);
    }

    public async Task UpdateCustomerAsync(int businessId, int customerId, CustomerDto customerDto)
    {
        if (customerDto == null)
        {
            throw new ArgumentNullException(nameof(customerDto), "CustomerDto cannot be null.");
        }

        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(customerId);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        // Update only the fields that are provided in the DTO or explicitly cleared
        if (customerDto.Email != null) // Check for null to update or clear
        {
            if (!string.IsNullOrWhiteSpace(customerDto.Email) && customerDto.Email != existingCustomer.Email)
            {
                if (!await _customerRepository.IsEmailUniqueAsync(customerDto.Email))
                {
                    throw new ArgumentException("Email is already in use.");
                }

                existingCustomer.Email = customerDto.Email; // Update email if provided
            }
            else if (customerDto.Email == string.Empty)
            {
                existingCustomer.Email = string.Empty; // Clear email if explicitly set to empty
            }
        }

        if (customerDto.Phone != null) // Check for null to update or clear
        {
            if (!string.IsNullOrWhiteSpace(customerDto.Phone) && customerDto.Phone != existingCustomer.Phone)
            {
                if (!await _customerRepository.IsPhoneUniqueAsync(customerDto.Phone))
                {
                    throw new ArgumentException("Phone is already in use.");
                }

                existingCustomer.Phone = customerDto.Phone; // Update phone if provided
            }
            else if (customerDto.Phone == string.Empty)
            {
                existingCustomer.Phone = string.Empty; // Clear phone if explicitly set to empty
            }
        }

        if (customerDto.Name != null) // Update or clear the name if provided
        {
            existingCustomer.Name = customerDto.Name; // Update the name if it's not null
        }

        // Update birthday only if it's not the minimum value (indicating it should be updated or cleared)
        if (customerDto.Birthday != default(DateTime))
        {
            existingCustomer.Birthday = customerDto.Birthday;
        }

        // Update wantsPromotion assuming it's non-nullable bool
        if (customerDto.WantsPromotion != null)
        {
            existingCustomer.WantsPromotion = (bool)customerDto.WantsPromotion;
        }

        // Update Note if provided, or keep the existing Note
        if (customerDto.Note != null)
        {
            existingCustomer.Note = customerDto.Note; // Update the note
        }
        
        await _customerRepository.UpdateCustomerAsync(existingCustomer);
    }


    public async Task DeleteCustomerAsync(int businessId, int customerId)
    {
        await _customerRepository.DeleteCustomerAsync(customerId);
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

    public async Task<IEnumerable<Customer?>> GetCustomersByBusinessIdAsync(int businessId)
    {
        return await _customerRepository.GetCustomersByBusinessIdAsync(businessId);
    }


    public async Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone)
    {
        return await _customerRepository.GetCustomerByNameOrPhoneAsync(nameOrPhone);
    }
}