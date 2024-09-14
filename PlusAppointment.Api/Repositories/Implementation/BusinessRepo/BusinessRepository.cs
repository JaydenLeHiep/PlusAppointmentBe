using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.BusinessRepo
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public BusinessRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<Business?>> GetAllAsync()
        {
            const string cacheKey = "all_businesses";
            var cachedBusinesses = await _redisHelper.GetCacheAsync<List<Business?>>(cacheKey);

            if (cachedBusinesses != null && cachedBusinesses.Any())
            {
                return cachedBusinesses;
            }

            var businesses = await _context.Businesses.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, businesses, TimeSpan.FromMinutes(10));

            return businesses;
        }

        public async Task<Business?> GetByIdAsync(int id)
        {
            string cacheKey = $"business_{id}";
            var cachedBusiness = await _redisHelper.GetCacheAsync<Business>(cacheKey);
            if (cachedBusiness != null)
            {
                return cachedBusiness;
            }

            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                throw new KeyNotFoundException($"Business with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, business, TimeSpan.FromMinutes(10));
            return business;
        }

        public async Task<Business?> GetByNameAsync(string businessName)
        {
            string cacheKey = $"business_name_{businessName.ToLower()}";
            var cachedBusiness = await _redisHelper.GetCacheAsync<Business>(cacheKey);
            if (cachedBusiness != null)
            {
                return cachedBusiness;
            }

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Name.ToLower() == businessName.ToLower());
            if (business == null)
            {
                throw new KeyNotFoundException($"Business with name {businessName} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, business, TimeSpan.FromMinutes(10));
            return business;
        }

        public async Task AddAsync(Business business)
        {
            await _context.Businesses.AddAsync(business);
            await _context.SaveChangesAsync();

            // Check if the business cache exists
            var businessCacheKey = $"business_{business.BusinessId}";
            var cachedBusiness = await _redisHelper.GetCacheAsync<Business>(businessCacheKey);

            if (cachedBusiness == null)
            {
                // If the cache is missing or expired, refresh the caches
                await RefreshRelatedCachesAsync(business);
            }
            else
            {
                // Otherwise, just update the existing cache
                await UpdateBusinessCacheAsync(business);
            }
        }

        public async Task UpdateAsync(Business business)
        {
            _context.Businesses.Update(business);
            await _context.SaveChangesAsync();

            // Check if the business cache exists
            var businessCacheKey = $"business_{business.BusinessId}";
            var cachedBusiness = await _redisHelper.GetCacheAsync<Business>(businessCacheKey);

            if (cachedBusiness == null)
            {
                // If the cache is missing or expired, refresh the caches
                await RefreshRelatedCachesAsync(business);
            }
            else
            {
                // Otherwise, just update the existing cache
                await UpdateBusinessCacheAsync(business);
            }
        }


        public async Task DeleteAsync(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business != null)
            {
                _context.Businesses.Remove(business);
                await _context.SaveChangesAsync();

                await InvalidateBusinessCacheAsync(business);
            }
        }

        public async Task<IEnumerable<Service?>> GetServicesByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"services_business_{businessId}";
            var cachedServices = await _redisHelper.GetCacheAsync<List<Service?>>(cacheKey);

            if (cachedServices != null && cachedServices.Any())
            {
                return cachedServices;
            }

            var services = await _context.Services
                .Where(s => s.BusinessId == businessId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, services, TimeSpan.FromMinutes(10));

            return services;
        }

        public async Task<IEnumerable<Staff?>> GetStaffByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"staff_business_{businessId}";
            var cachedStaff = await _redisHelper.GetCacheAsync<List<Staff?>>(cacheKey);

            if (cachedStaff != null && cachedStaff.Any())
            {
                return cachedStaff;
            }

            var staff = await _context.Staffs.Where(s => s.BusinessId == businessId).ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, staff, TimeSpan.FromMinutes(10));

            return staff;
        }

        public async Task<IEnumerable<Business?>> GetAllByUserIdAsync(int userId)
        {
            string cacheKey = $"business_user_{userId}";
            var cachedBusinesses = await _redisHelper.GetCacheAsync<List<Business>>(cacheKey);

            if (cachedBusinesses != null && cachedBusinesses.Any())
            {
                return cachedBusinesses;
            }

            var businesses = await _context.Businesses.Where(b => b.UserID == userId).ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, businesses, TimeSpan.FromMinutes(10));

            return businesses;
        }
        
        private async Task RefreshRelatedCachesAsync(Business business)
        {
            // Refresh individual business cache
            var businessCacheKey = $"business_{business.BusinessId}";
            await _redisHelper.SetCacheAsync(businessCacheKey, business, TimeSpan.FromMinutes(10));

            // Refresh the user's business list cache
            string userBusinessCacheKey = $"business_user_{business.UserID}";
            var userBusinesses = await _context.Businesses
                .Where(b => b.UserID == business.UserID)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(userBusinessCacheKey, userBusinesses, TimeSpan.FromMinutes(10));

            // Refresh the cache for all businesses
            const string allBusinessesCacheKey = "all_businesses";
            var allBusinesses = await _context.Businesses.ToListAsync();
            await _redisHelper.SetCacheAsync(allBusinessesCacheKey, allBusinesses, TimeSpan.FromMinutes(10));
        }


        private async Task UpdateBusinessCacheAsync(Business business)
        {
            var businessCacheKey = $"business_{business.BusinessId}";
            await _redisHelper.SetCacheAsync(businessCacheKey, business, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<Business>(
                $"business_user_{business.UserID}",
                list =>
                {
                    list.RemoveAll(b => b.BusinessId == business.BusinessId);
                    list.Add(business);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateBusinessCacheAsync(Business business)
        {
            var businessCacheKey = $"business_{business.BusinessId}";
            await _redisHelper.DeleteCacheAsync(businessCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<Business>(
                $"business_user_{business.UserID}",
                list =>
                {
                    list.RemoveAll(b => b.BusinessId == business.BusinessId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }
    }
}
