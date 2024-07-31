using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.CustomerRepo
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public CustomerRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<Customer?>> GetAllCustomersAsync()
        {
            const string cacheKey = "all_customers";
            var cachedCustomers = await _redisHelper.GetCacheAsync<List<Customer?>>(cacheKey);

            if (cachedCustomers != null && cachedCustomers.Any())
            {
                return cachedCustomers;
            }

            var customers = await _context.Customers.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, customers, TimeSpan.FromMinutes(10));

            return customers;
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            string cacheKey = $"customer_{customerId}";
            var customer = await _redisHelper.GetCacheAsync<Customer>(cacheKey);
            if (customer != null)
            {
                return customer;
            }

            customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
            return customer;
        }

        public async Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone)
        {
            string cacheKey = $"customer_emailOrPhone_{emailOrPhone}";
            var customer = await _redisHelper.GetCacheAsync<Customer>(cacheKey);
            if (customer != null)
            {
                return customer;
            }

            customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == emailOrPhone || c.Phone == emailOrPhone);
            if (customer == null)
            {
                return null;
            }

            await _redisHelper.SetCacheAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
            return customer;
        }

        public async Task AddCustomerAsync(Customer? customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            await UpdateCustomerCacheAsync(customer);
        }

        public async Task UpdateCustomerAsync(Customer? customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            await UpdateCustomerCacheAsync(customer);
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                await InvalidateCustomerCacheAsync(customer);
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            }

            return !await _context.Customers.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                throw new ArgumentNullException(nameof(phone), "Phone number cannot be null or empty.");
            }

            return !await _context.Customers.AnyAsync(c => c.Phone == phone);
        }

        private async Task UpdateCustomerCacheAsync(Customer customer)
        {
            var customerCacheKey = $"customer_{customer.CustomerId}";
            await _redisHelper.SetCacheAsync(customerCacheKey, customer, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<Customer>(
                "all_customers",
                list =>
                {
                    list.RemoveAll(c => c.CustomerId == customer.CustomerId);
                    list.Add(customer);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateCustomerCacheAsync(Customer customer)
        {
            var customerCacheKey = $"customer_{customer.CustomerId}";
            await _redisHelper.DeleteCacheAsync(customerCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<Customer>(
                "all_customers",
                list =>
                {
                    list.RemoveAll(c => c.CustomerId == customer.CustomerId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }
    }
}