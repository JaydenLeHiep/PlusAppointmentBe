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
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly RedisHelper _redisHelper;

        public ServicesRepository(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
        {
            _contextFactory = contextFactory;
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

            using (var context = _contextFactory.CreateDbContext())
            {
                var services = await context.Services.Include(s => s.Category).ToListAsync();
                await _redisHelper.SetCacheAsync(cacheKey, services, TimeSpan.FromMinutes(10));
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
            string cacheKey = $"service_business_{businessId}";
            var cachedServices = await _redisHelper.GetCacheAsync<List<Service>>(cacheKey);

            if (cachedServices != null && cachedServices.Any())
            {
                return cachedServices.OrderBy(s => s.ServiceId);
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var services = await context.Services
                    .Include(s => s.Category)
                    .Where(s => s.BusinessId == businessId)
                    .OrderBy(s => s.ServiceId)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(cacheKey, services, TimeSpan.FromMinutes(10));
                return services;
            }
        }

        public async Task AddServiceAsync(Service? service, int businessId)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    throw new Exception("Business not found");
                }

                await context.Services.AddAsync(service);
                await context.SaveChangesAsync();
            }

            await RefreshRelatedCachesAsync(service);
        }

        public async Task AddListServicesAsync(IEnumerable<Service?> services, int businessId)
        {
            if (!services.Any()) return;

            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    throw new Exception("Business not found");
                }

                await context.Services.AddRangeAsync(services!);
                await context.SaveChangesAsync();
            }

            foreach (var service in services)
            {
                if (service != null)
                {
                    await UpdateServiceCacheAsync(service);
                }
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

                await UpdateServiceCacheAsync(service);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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

                await _redisHelper.SetCacheAsync(cacheKey, service, TimeSpan.FromMinutes(10));
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
                    await InvalidateServiceCacheAsync(service);
                    await RefreshRelatedCachesAsync(service);
                }
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
                    int index = list.FindIndex(s => s.ServiceId == service.ServiceId);
                    if (index != -1)
                    {
                        list[index] = service;
                    }
                    else
                    {
                        list.Add(service);
                    }

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
            using (var context = _contextFactory.CreateDbContext())
            {
                var serviceCacheKey = $"service_{service.ServiceId}";
                await _redisHelper.SetCacheAsync(serviceCacheKey, service, TimeSpan.FromMinutes(10));

                string businessCacheKey = $"service_business_{service.BusinessId}";
                var businessServices = await context.Services
                    .Include(s => s.Category)
                    .Where(s => s.BusinessId == service.BusinessId)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(businessCacheKey, businessServices, TimeSpan.FromMinutes(10));

                const string allServicesCacheKey = "all_services";
                var allServices = await context.Services.Include(s => s.Category).ToListAsync();
                await _redisHelper.SetCacheAsync(allServicesCacheKey, allServices, TimeSpan.FromMinutes(10));
            }
        }
    }
}
