using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Appointment;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;

namespace PlusAppointment.Repositories.Implementation.CustomerRepo
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CustomerRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Customer?>> GetAllCustomersAsync()
        {
            
            using (var context = _contextFactory.CreateDbContext())
            {
                var customers = await context.Customers.ToListAsync();
                return customers;
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {

            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }
                
                return customer;
            }
        }

        public async Task<Customer?> GetCustomerByEmailOrPhoneAsync(string emailOrPhone)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c => c.Email == emailOrPhone || c.Phone == emailOrPhone);
                if (customer == null)
                {
                    return null;
                }
                
                return customer;
            }
        }

        public async Task<Customer?> GetCustomerByEmailOrPhoneAndBusinessIdAsync(string emailOrPhone, int businessId)
        {


            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c =>
                        (c.Email.ToLower() == emailOrPhone.ToLower() || c.Phone == emailOrPhone) && c.BusinessId == businessId);

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
                        note = @Note,
                        business_id = @BusinessId
                    WHERE customer_id = @CustomerId";

                await using var command = new NpgsqlCommand(updateQuery, connection, transaction);
                
                command.Parameters.AddWithValue("@Name", (object)customer.Name ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (object)customer.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object)customer.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Birthday", customer.Birthday ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@WantsPromotion", customer.WantsPromotion);
                command.Parameters.AddWithValue("@Note", (object)customer.Note ?? DBNull.Value);
                command.Parameters.AddWithValue("@BusinessId", customer.BusinessId);
                command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

                await command.ExecuteNonQueryAsync();
                
                await transaction.CommitAsync();
            }
            catch
            {
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
                }
            }
        }

        public async Task<bool> IsEmailUniqueAsync(int businessId, string email)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return !await context.Customers.AnyAsync(c => c.Email.ToLower() == email.ToLower() && c.BusinessId == businessId);
            }
        }

        public async Task<bool> IsPhoneUniqueAsync(int businessId, string phone)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return !await context.Customers.AnyAsync(c => c.Phone.ToLower() == phone.ToLower() && c.BusinessId == businessId);
            }
        }


        public async Task<IEnumerable<AppointmentHistoryDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {

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
                return appointments;
            }
        }

        public async Task<IEnumerable<Customer?>> GetCustomersByBusinessIdAsync(int businessId)
        {


            using (var context = _contextFactory.CreateDbContext())
            {
                var customers = await context.Customers
                    .Where(c => c.BusinessId == businessId)
                    .ToListAsync();
                
                return customers;
            }
        }

        public async Task<IEnumerable<Customer?>> SearchCustomersByNameOrPhoneAsync(string searchTerm)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var customers = await context.Customers
                    .Where(c => c != null &&
                                (c.Name != null && c.Name.ToLower().Contains(searchTerm.ToLower()) ||
                                 c.Phone != null && c.Phone.Contains(searchTerm)))
                    .ToListAsync();

                return customers;
            }
        }

        public async Task<Customer?> GetCustomerByNameOrPhoneAsync(string nameOrPhone)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c => c.Name == nameOrPhone || c.Phone == nameOrPhone);
                
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
    }
}