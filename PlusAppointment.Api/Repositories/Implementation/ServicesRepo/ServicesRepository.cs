using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;

namespace PlusAppointment.Repositories.Implementation.ServicesRepo
{
    public class ServicesRepository : IServicesRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ServicesRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Service?>> GetAllAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var services = await context.Services.Include(s => s.Category).ToListAsync();
                return services;
            }
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var service = await context.Services.Include(s => s.Category).FirstOrDefaultAsync(s => s.ServiceId == id);
                if (service == null)
                {
                    throw new KeyNotFoundException($"Service with ID {id} not found");
                }

                return service;
            }
        }

        public async Task<IEnumerable<Service?>> GetAllByBusinessIdAsync(int businessId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var services = await context.Services
                    .Include(s => s.Category)
                    .Where(s => s.BusinessId == businessId)
                    .OrderBy(s => s.ServiceId)
                    .ToListAsync();

                return services;
            }
        }

        public async Task AddServiceAsync(Service? service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(service.BusinessId);
                if (business == null)
                {
                    throw new KeyNotFoundException($"Business with ID {service.BusinessId} not found.");
                }

                await context.Services.AddAsync(service);
                await context.SaveChangesAsync();
            }
        }


        public async Task AddListServicesAsync(IEnumerable<Service?> services)
        {
            var enumerable = services.ToList();
            if (!enumerable.Any()) return;

            using (var context = _contextFactory.CreateDbContext())
            {
                await context.Services.AddRangeAsync(enumerable!);
                await context.SaveChangesAsync();
            }
        }


        public async Task UpdateAsync(Service service)
        {
            await using var connection = new NpgsqlConnection(_contextFactory.CreateDbContext().Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var updateQuery = @"
                    UPDATE services 
                    SET 
                        name = @Name, 
                        description = @Description, 
                        duration = @Duration, 
                        price = @Price, 
                        category_id = @CategoryId
                    WHERE service_id = @ServiceId";

                await using var command = new NpgsqlCommand(updateQuery, connection, transaction);

                command.Parameters.AddWithValue("@Name", service.Name);
                command.Parameters.AddWithValue("@Description", service.Description);
                command.Parameters.AddWithValue("@Duration", NpgsqlDbType.Interval, service.Duration);
                command.Parameters.AddWithValue("@Price", service.Price);
                command.Parameters.AddWithValue("@CategoryId", service.CategoryId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceId", service.ServiceId);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Service?> GetByBusinessIdServiceIdAsync(int businessId, int serviceId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var service = await context.Services
                    .Include(s => s.Category)
                    .Where(s => s.BusinessId == businessId && s.ServiceId == serviceId)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    throw new KeyNotFoundException($"Service with ID {serviceId} not found for Business ID {businessId}");
                }
                
                return service;
            }
        }

        public async Task DeleteAsync(int businessId, int serviceId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var service = await context.Services
                    .Where(s => s.BusinessId == businessId && s.ServiceId == serviceId)
                    .FirstOrDefaultAsync();

                if (service != null)
                {
                    context.Services.Remove(service);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
