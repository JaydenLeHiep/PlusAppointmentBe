using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using WebApplication1.Data;
using WebApplication1.Repositories.Interfaces.ServicesRepo;
using WebApplication1.Utils.Redis;

namespace WebApplication1.Repositories.Implementation.ServicesRepo
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
            var cachedServices = await _redisHelper.GetCacheAsync<List<Service?>>(cacheKey);

            if (cachedServices != null && cachedServices.Any())
            {
                return cachedServices;
            }

            var services = await _context.Services.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, services, TimeSpan.FromMinutes(10));

            return services;
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            string cacheKey = $"service_{id}";
            var service = await _redisHelper.GetCacheAsync<Service>(cacheKey);
            if (service != null)
            {
                return service;
            }

            service = await _context.Services.FindAsync(id);
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
                return cachedServices;
            }

            var services = await _context.Services.Where(s => s.BusinessId == businessId).ToListAsync();
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
                    await UpdateServiceCacheAsync(service);
                }
            }
        }

        public async Task<Service?> GetByBusinessIdServiceIdAsync(int businessId, int serviceId)
        {
            return await _context.Services
                .Where(s => s.BusinessId == businessId && s.ServiceId == serviceId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            await UpdateServiceCacheAsync(service);
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
                    list.RemoveAll(s => s.ServiceId == service.ServiceId);
                    list.Add(service);
                    return list;
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
    }
}
