using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.ServicesRepo
{
    public class ServicesRepository : IServicesRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public ServicesRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<Service?>> GetAllAsync()
        {
            const string cacheKey = "all_services";
            var cachedServices = await _redisHelper.GetCacheAsync<List<Service>>(cacheKey);

            if (cachedServices != null && cachedServices.Any())
            {
                return cachedServices;
            }

            var services = await _context.Services.Include(s => s.Category).ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, services, TimeSpan.FromMinutes(10));

            return services;
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            string cacheKey = $"service_{id}";
            var cachedService = await _redisHelper.GetCacheAsync<Service>(cacheKey);
            if (cachedService != null)
            {
                return cachedService;
            }

            var service = await _context.Services.Include(s => s.Category).FirstOrDefaultAsync(s => s.ServiceId == id);
            if (service == null)
            {
                throw new KeyNotFoundException($"Service with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, service, TimeSpan.FromMinutes(10));
            return service;
        }

        public async Task<IEnumerable<Service?>> GetAllByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"service_business_{businessId}";
            var cachedServices = await _redisHelper.GetCacheAsync<List<Service>>(cacheKey);

            if (cachedServices != null && cachedServices.Any())
            {
                // Ensure the cached services are sorted by ServiceId before returning
                return cachedServices.OrderBy(s => s.ServiceId);
            }

            var services = await _context.Services
                .Include(s => s.Category)
                .Where(s => s.BusinessId == businessId)
                .OrderBy(s => s.ServiceId) // Sort the services by ServiceId when fetching from the database
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, services, TimeSpan.FromMinutes(10));

            return services;
        }


        public async Task AddServiceAsync(Service? service, int businessId)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null)
            {
                throw new Exception("Business not found");
            }
            if (service == null)
            {
                throw new Exception("Service not found");
            }

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            await UpdateServiceCacheAsync(service);
            // Refresh related caches
            await RefreshRelatedCachesAsync(service);
        }

        public async Task AddListServicesAsync(IEnumerable<Service?> services, int businessId)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null)
            {
                throw new Exception("Business not found");
            }

            await _context.Services.AddRangeAsync(services!);
            await _context.SaveChangesAsync();

            foreach (var service in services)
            {
                if (service != null)
                {
                    await RefreshRelatedCachesAsync(service);
                }
            }
        }

        public async Task<Service?> GetByBusinessIdServiceIdAsync(int businessId, int serviceId)
        {
            string cacheKey = $"service_{serviceId}";
            var cachedService = await _redisHelper.GetCacheAsync<Service>(cacheKey);
            if (cachedService != null)
            {
                return cachedService;
            }

            var service = await _context.Services
                .Include(s => s.Category) // Include the category when fetching the service
                .Where(s => s.BusinessId == businessId && s.ServiceId == serviceId)
                .FirstOrDefaultAsync();

            if (service == null)
            {
                throw new KeyNotFoundException($"Service with ID {serviceId} not found for Business ID {businessId}");
            }

            await _redisHelper.SetCacheAsync(cacheKey, service, TimeSpan.FromMinutes(10));
            return service;
        }

        public async Task UpdateAsync(Service service)
        {
            await using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // SQL query to update the service in PostgreSQL
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

                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@Name", service.Name);
                command.Parameters.AddWithValue("@Description", service.Description);
                command.Parameters.AddWithValue("@Duration", NpgsqlDbType.Interval, service.Duration);
                command.Parameters.AddWithValue("@Price", service.Price);
                command.Parameters.AddWithValue("@CategoryId", service.CategoryId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceId", service.ServiceId);

                // Execute the update query
                await command.ExecuteNonQueryAsync();

                // Commit transaction after successful update
                await transaction.CommitAsync();

                // After updating the database, refresh the cache
                await UpdateServiceCacheAsync(service);
                await RefreshRelatedCachesAsync(service);
            }
            catch
            {
                // Rollback transaction in case of any error
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task DeleteAsync(int businessId, int serviceId)
        {
            var service = await _context.Services
                .Where(s => s.BusinessId == businessId && s.ServiceId == serviceId)
                .FirstOrDefaultAsync();

            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                await InvalidateServiceCacheAsync(service);
                // Refresh related caches
                await RefreshRelatedCachesAsync(service);
            }
        }

        private async Task UpdateServiceCacheAsync(Service service)
        {
            var serviceCacheKey = $"service_{service.ServiceId}";
            await _redisHelper.SetCacheAsync(serviceCacheKey, service, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<Service>(
                $"service_business_{service.BusinessId}",
                list =>
                {
                    // Find the index of the existing service in the list
                    int index = list.FindIndex(s => s.ServiceId == service.ServiceId);
                    if (index != -1)
                    {
                        // Replace the existing service with the updated one
                        list[index] = service;
                    }
                    else
                    {
                        // If the service is not found in the list, add it to the list
                        list.Add(service);
                    }

                    // Sort the list by ServiceId
                    return list.OrderBy(s => s.ServiceId).ToList();
                },
                TimeSpan.FromMinutes(10));
        }



        private async Task InvalidateServiceCacheAsync(Service service)
        {
            var serviceCacheKey = $"service_{service.ServiceId}";
            await _redisHelper.DeleteCacheAsync(serviceCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<Service>(
                $"service_business_{service.BusinessId}",
                list =>
                {
                    list.RemoveAll(s => s.ServiceId == service.ServiceId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }
        
        private async Task RefreshRelatedCachesAsync(Service service)
        {
            // Refresh individual Service cache
            var serviceCacheKey = $"service_{service.ServiceId}";
            await _redisHelper.SetCacheAsync(serviceCacheKey, service, TimeSpan.FromMinutes(10));

            // Refresh list of all Services for the Business
            string businessCacheKey = $"service_business_{service.BusinessId}";
            var businessServices = await _context.Services
                .Include(s => s.Category)
                .Where(s => s.BusinessId == service.BusinessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(businessCacheKey, businessServices, TimeSpan.FromMinutes(10));

            // Optionally refresh the cache for all services if required
            const string allServicesCacheKey = "all_services";
            var allServices = await _context.Services.Include(s => s.Category).ToListAsync();
            await _redisHelper.SetCacheAsync(allServicesCacheKey, allServices, TimeSpan.FromMinutes(10));
        }

    }
}
