using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using WebApplication1.Data;
using WebApplication1.Repositories.Interfaces.CustomerRepo;
using WebApplication1.Utils.Redis;


namespace WebApplication1.Repositories.Implementation.CustomerRepo
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

        public async Task AddCustomerAsync(Customer? customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            await InvalidateCache();
        }

        public async Task UpdateCustomerAsync(Customer? customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            await InvalidateCache();
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                await InvalidateCache();
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            }

            return !await _context.Customers.AnyAsync(c => c != null && c.Email == email);
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                throw new ArgumentNullException(nameof(phone), "Phone number cannot be null or empty.");
            }

            return !await _context.Customers.AnyAsync(c => c != null && c.Phone == phone);
        }

        private async Task InvalidateCache()
        {
            await _redisHelper.DeleteKeysByPatternAsync("customer_*");
            await _redisHelper.DeleteCacheAsync("all_customers");
        }
    }
}