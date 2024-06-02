using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using WebApplication1.Data;
using WebApplication1.Repositories.Interfaces.CustomerRepo;


namespace WebApplication1.Repositories.Implementation.CustomerRepo
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers.ToListAsync();

            return customers.Select(a => new CustomerDto
            {
                Name = a.Name,
                Email = a.Email,
                Phone = a.Phone
            }).ToList();
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(a => a.CustomerId == customerId);
            if (customer == null)
            {
                return null;
            }

            return new CustomerDto
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone
            };
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Customers.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone)
        {
            return !await _context.Customers.AnyAsync(c => c.Phone == phone);
        }
    }
}