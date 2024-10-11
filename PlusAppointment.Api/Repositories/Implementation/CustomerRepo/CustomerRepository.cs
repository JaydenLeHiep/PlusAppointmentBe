using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.CustomerRepo
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly RedisHelper _redisHelper;

        public CustomerRepository(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
        {
            _contextFactory = contextFactory;
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

            using (var context = _contextFactory.CreateDbContext())
            {
                var customers = await context.Customers.ToListAsync();
                await _redisHelper.SetCacheAsync(cacheKey, customers, TimeSpan.FromMinutes(10));
                return customers;
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            string cacheKey = $"customer_{customerId}";
            var cachedCustomer = await _redisHelper.GetCacheAsync<Customer>(cacheKey);
            if (cachedCustomer != null)
            {
                return cachedCustomer;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }

                await _redisHelper.SetCacheAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
                return customer;
            }
        }

        public async Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone)
        {
            string cacheKey = $"customer_emailOrPhone_{emailOrPhone}";
            var cachedCustomer = await _redisHelper.GetCacheAsync<Customer>(cacheKey);
            if (cachedCustomer != null)
            {
                return cachedCustomer;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c => c.Email == emailOrPhone || c.Phone == emailOrPhone);
                if (customer == null)
                {
                    return null;
                }

                await _redisHelper.SetCacheAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
                return customer;
            }
        }

        public async Task<Customer?> GetCustomerByEmailOrPhoneAndBusinessIdAsync(string emailOrPhone, int businessId)
        {
            string cacheKey = $"customer_emailOrPhone_{emailOrPhone}_business_{businessId}";
            var cachedCustomer = await _redisHelper.GetCacheAsync<Customer>(cacheKey);
            if (cachedCustomer != null)
            {
                return cachedCustomer;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c =>
                        (c.Email == emailOrPhone || c.Phone == emailOrPhone) && c.BusinessId == businessId);

                if (customer != null)
                {
                    await _redisHelper.SetCacheAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
                }

                return customer;
            }
        }

        public async Task AddCustomerAsync(Customer? customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }

            await RefreshRelatedCachesAsync(customer);
        }

        public async Task UpdateCustomerAsync(Customer? customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            await using var connection =
                new NpgsqlConnection(_contextFactory.CreateDbContext().Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // SQL query to update the customer in PostgreSQL
                var updateQuery = @"
                    UPDATE customers
                    SET 
                        name = @Name, 
                        email = @Email, 
                        phone = @Phone,
                        birthday = @Birthday, 
                        wants_promotion = @WantsPromotion,
                        business_id = @BusinessId
                    WHERE customer_id = @CustomerId";

                await using var command = new NpgsqlCommand(updateQuery, connection, transaction);

                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@Name", (object)customer.Name ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (object)customer.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object)customer.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Birthday", customer.Birthday ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@WantsPromotion", customer.WantsPromotion);
                command.Parameters.AddWithValue("@BusinessId", customer.BusinessId);
                command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

                // Execute the update query
                await command.ExecuteNonQueryAsync();

                // Commit the transaction after a successful update
                await transaction.CommitAsync();

                await RefreshRelatedCachesAsync(customer);
            }
            catch
            {
                // Rollback transaction in case of any error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers.FindAsync(customerId);
                if (customer != null)
                {
                    context.Customers.Remove(customer);
                    await context.SaveChangesAsync();

                    await InvalidateCustomerCacheAsync(customer);
                    await RefreshRelatedCachesAsync(customer);
                }
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return !await context.Customers.AnyAsync(c => c.Email.ToLower() == email.ToLower());
            }
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return !await context.Customers.AnyAsync(c => c.Phone.ToLower() == phone.ToLower());
            }
        }

        public async Task<IEnumerable<AppointmentHistoryDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            string cacheKey = $"customer_{customerId}_appointments";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentHistoryDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var appointments = await context.Appointments
                    .Where(a => a.CustomerId == customerId)
                    .Select(a => new AppointmentHistoryDto
                    {
                        AppointmentTime = a.AppointmentTime,
                        StaffServices = a.AppointmentServices!.Select(ass => new StaffServiceDto
                        {
                            StaffName = ass.Staff!.Name,
                            ServiceName = ass.Service!.Name
                        }).ToList()
                    })
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(cacheKey, appointments, TimeSpan.FromMinutes(10));
                return appointments;
            }
        }

        public async Task<IEnumerable<Customer?>> GetCustomersByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"customers_business_{businessId}";
            var cachedCustomers = await _redisHelper.GetCacheAsync<List<Customer?>>(cacheKey);

            if (cachedCustomers != null && cachedCustomers.Any())
            {
                return cachedCustomers.OrderBy(c => c.CustomerId);
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var customers = await context.Customers
                    .Where(c => c.BusinessId == businessId)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(cacheKey, customers, TimeSpan.FromMinutes(10));
                return customers;
            }
        }

        public async Task<IEnumerable<Customer?>> SearchCustomersByNameOrPhoneAsync(string searchTerm)
        {
            string cacheKey = $"customer_search_{searchTerm.ToLower()}";
            var cachedCustomers = await _redisHelper.GetCacheAsync<List<Customer?>>(cacheKey);

            if (cachedCustomers != null && cachedCustomers.Any())
            {
                return cachedCustomers;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var customers = await context.Customers
                    .Where(c => c != null &&
                                (c.Name != null && c.Name.ToLower().Contains(searchTerm.ToLower()) ||
                                 c.Phone != null && c.Phone.Contains(searchTerm)))
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(cacheKey, customers, TimeSpan.FromMinutes(10));
                return customers;
            }
        }

        public async Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone)
        {
            string cacheKey = $"customer_nameOrPhone_{nameOrPhone.ToLower()}";
            var cachedCustomer = await _redisHelper.GetCacheAsync<Customer>(cacheKey);
            if (cachedCustomer != null)
            {
                return cachedCustomer;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c => c.Name == nameOrPhone || c.Phone == nameOrPhone);

                await _redisHelper.SetCacheAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
                return customer;
            }
        }
        
        public async Task<IEnumerable<Customer?>> GetCustomersWithUpcomingBirthdayAsync(DateTime date)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var today = date.Date;
                return await context.Customers
                    .Where(c => c.Birthday.HasValue &&
                                c.Birthday.Value.Month == today.Month &&
                                c.Birthday.Value.Day == today.Day &&
                                c.WantsPromotion)
                    .ToListAsync();
            }
        }

        private async Task UpdateCustomerCacheAsync(Customer customer)
        {
            var customerCacheKey = $"customer_{customer.CustomerId}";
            await _redisHelper.SetCacheAsync(customerCacheKey, customer, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<Customer>(
                $"customers_business_{customer.BusinessId}",
                list =>
                {
                    int index = list.FindIndex(c => c.CustomerId == customer.CustomerId);
                    if (index != -1)
                    {
                        list[index] = customer;
                    }
                    else
                    {
                        list.Add(customer);
                    }

                    return list.OrderBy(c => c.CustomerId).ToList();
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateCustomerCacheAsync(Customer customer)
        {
            var customerCacheKey = $"customer_{customer.CustomerId}";
            await _redisHelper.DeleteCacheAsync(customerCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<Customer>(
                $"customers_business_{customer.BusinessId}",
                list =>
                {
                    list.RemoveAll(c => c.CustomerId == customer.CustomerId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(Customer customer)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var customerCacheKey = $"customer_{customer.CustomerId}";
                await _redisHelper.SetCacheAsync(customerCacheKey, customer, TimeSpan.FromMinutes(10));

                string businessCacheKey = $"customers_business_{customer.BusinessId}";
                var businessCustomers = await context.Customers
                    .Where(c => c.BusinessId == customer.BusinessId)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(businessCacheKey, businessCustomers, TimeSpan.FromMinutes(10));

                const string allCustomersCacheKey = "all_customers";
                var allCustomers = await context.Customers.ToListAsync();
                await _redisHelper.SetCacheAsync(allCustomersCacheKey, allCustomers, TimeSpan.FromMinutes(10));
            }
        }
    }
}