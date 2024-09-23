using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.IOpeningHoursRepository;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.OpeningHoursRepository
{
    public class OpeningHoursRepository(ApplicationDbContext context, RedisHelper redisHelper) : IOpeningHoursRepository
    {
        public async Task<OpeningHours?> GetByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"opening_hours_business_{businessId}";
            
            // Attempt to get from cache
            var cachedHours = await redisHelper.GetCacheAsync<OpeningHours>(cacheKey);
            if (cachedHours != null)
            {
                return cachedHours;
            }

            // If not in cache, retrieve from database
            var openingHours = await context.OpeningHours
                .FirstOrDefaultAsync(oh => oh.BusinessId == businessId);

            if (openingHours != null)
            {
                // Set cache
                await redisHelper.SetCacheAsync(cacheKey, openingHours, TimeSpan.FromMinutes(10));
            }

            return openingHours;
        }

        public async Task AddAsync(OpeningHours openingHours)
        {
            await context.OpeningHours.AddAsync(openingHours);
            await context.SaveChangesAsync();

            // Directly update the cache after adding
            await RefreshRelatedCachesAsync(openingHours);
        }

        public async Task UpdateAsync(OpeningHours openingHours)
        {
            context.OpeningHours.Update(openingHours);
            await context.SaveChangesAsync();

            // Update the cache immediately after modification
            await UpdateOpeningHoursCacheAsync(openingHours);
        }

        public async Task DeleteAsync(int businessId)
        {
            var openingHours = await GetByBusinessIdAsync(businessId);
            if (openingHours != null)
            {
                context.OpeningHours.Remove(openingHours);
                await context.SaveChangesAsync();

                // Invalidate the cache after deletion
                await InvalidateOpeningHoursCacheAsync(openingHours);
            }
        }

        private async Task UpdateOpeningHoursCacheAsync(OpeningHours openingHours)
        {
            var cacheKey = $"opening_hours_business_{openingHours.BusinessId}";
            await redisHelper.SetCacheAsync(cacheKey, openingHours, TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateOpeningHoursCacheAsync(OpeningHours openingHours)
        {
            var cacheKey = $"opening_hours_business_{openingHours.BusinessId}";
            await redisHelper.DeleteCacheAsync(cacheKey);
        }

        private async Task RefreshRelatedCachesAsync(OpeningHours openingHours)
        {
            var cacheKey = $"opening_hours_business_{openingHours.BusinessId}";
            
            // Refresh the cache with the latest data from the database
            var latestOpeningHours = await context.OpeningHours
                .FirstOrDefaultAsync(oh => oh.BusinessId == openingHours.BusinessId);

            if (latestOpeningHours != null)
            {
                await redisHelper.SetCacheAsync(cacheKey, latestOpeningHours, TimeSpan.FromMinutes(10));
            }
            else
            {
                // If no data is found in the database, ensure the cache is cleared
                await redisHelper.DeleteCacheAsync(cacheKey);
            }
        }
    }
}