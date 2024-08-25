using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ServiceCategoryRepo;
using PlusAppointment.Utils.Redis;


namespace PlusAppointment.Repositories.Implementation.ServiceCategoryRepo
{
    public class ServiceCategoryRepo : IServiceCategoryRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public ServiceCategoryRepo(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<ServiceCategory?> GetServiceCategoryByIdAsync(int id)
        {
            string cacheKey = $"service_category_{id}";
            var serviceCategory = await _redisHelper.GetCacheAsync<ServiceCategory>(cacheKey);

            if (serviceCategory != null)
            {
                return serviceCategory;
            }

            serviceCategory = await _context.ServiceCategories.FindAsync(id);
            if (serviceCategory == null)
            {
                throw new KeyNotFoundException($"ServiceCategory with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, serviceCategory, TimeSpan.FromMinutes(10));
            return serviceCategory;
        }

        public async Task<IEnumerable<ServiceCategory>> GetAllServiceCategoriesAsync()
        {
            const string cacheKey = "all_service_categories";
            var cachedServiceCategories = await _redisHelper.GetCacheAsync<List<ServiceCategory>>(cacheKey);

            if (cachedServiceCategories != null && cachedServiceCategories.Any())
            {
                return cachedServiceCategories;
            }

            var serviceCategories = await _context.ServiceCategories.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, serviceCategories, TimeSpan.FromMinutes(10));

            return serviceCategories;
        }

        public async Task AddServiceCategoryAsync(ServiceCategory serviceCategory)
        {
            _context.ServiceCategories.Add(serviceCategory);
            await _context.SaveChangesAsync();

            await UpdateServiceCategoryCacheAsync(serviceCategory);
        }

        public async Task UpdateServiceCategoryAsync(ServiceCategory serviceCategory)
        {
            _context.ServiceCategories.Update(serviceCategory);
            await _context.SaveChangesAsync();

            await UpdateServiceCategoryCacheAsync(serviceCategory);
        }

        public async Task DeleteServiceCategoryAsync(int id)
        {
            var serviceCategory = await _context.ServiceCategories.FindAsync(id);
            if (serviceCategory != null)
            {
                _context.ServiceCategories.Remove(serviceCategory);
                await _context.SaveChangesAsync();
                await InvalidateServiceCategoryCacheAsync(serviceCategory);
            }
        }

        private async Task UpdateServiceCategoryCacheAsync(ServiceCategory serviceCategory)
        {
            var serviceCategoryCacheKey = $"service_category_{serviceCategory.CategoryId}";
            await _redisHelper.SetCacheAsync(serviceCategoryCacheKey, serviceCategory, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<ServiceCategory>(
                "all_service_categories",
                list =>
                {
                    list.RemoveAll(sc => sc.CategoryId == serviceCategory.CategoryId);
                    list.Add(serviceCategory);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateServiceCategoryCacheAsync(ServiceCategory serviceCategory)
        {
            var serviceCategoryCacheKey = $"service_category_{serviceCategory.CategoryId}";
            await _redisHelper.DeleteCacheAsync(serviceCategoryCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<ServiceCategory>(
                "all_service_categories",
                list =>
                {
                    list.RemoveAll(sc => sc.CategoryId == serviceCategory.CategoryId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }
    }
}
