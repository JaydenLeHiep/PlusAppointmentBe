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
            var cachedHours = await redisHelper.GetCacheAsync<OpeningHours>(cacheKey);

            if (cachedHours != null)
            {
                return cachedHours;
            }

            var openingHours = await context.OpeningHours
                .FirstOrDefaultAsync(oh => oh.BusinessId == businessId);

            if (openingHours != null)
            {
                await redisHelper.SetCacheAsync(cacheKey, openingHours, TimeSpan.FromMinutes(10));
            }

            return openingHours;
        }

        public async Task AddAsync(OpeningHours openingHours)
        {
            await context.OpeningHours.AddAsync(openingHours);
            await context.SaveChangesAsync();

            // Update cache after adding
            await RefreshRelatedCachesAsync(openingHours);
        }

        public async Task UpdateAsync(OpeningHours openingHours)
        {
            context.OpeningHours.Update(openingHours);
            await context.SaveChangesAsync();

            // Update the cache
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
            await redisHelper.SetCacheAsync(cacheKey, openingHours, TimeSpan.FromMinutes(10));

            // Here you might want to refresh related data if needed in a more complex scenario
            var allOpeningHours = await context.OpeningHours
                .Where(oh => oh.BusinessId == openingHours.BusinessId)
                .ToListAsync();

            await redisHelper.SetCacheAsync(cacheKey, allOpeningHours, TimeSpan.FromMinutes(10));
        }
    }
}