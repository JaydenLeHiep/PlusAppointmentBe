using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotAvailableTimeRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.NotAvailableTimeRepo
{
    public class NotAvailableTimeRepository : INotAvailableTimeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public NotAvailableTimeRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<NotAvailableTime>> GetAllByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"not_available_times_business_{businessId}";
            var cachedTimes = await _redisHelper.GetCacheAsync<List<NotAvailableTime>>(cacheKey);

            if (cachedTimes != null && cachedTimes.Any())
            {
                return cachedTimes;
            }

            var notAvailableTimes = await _context.NotAvailableTimes
                .Where(nat => nat.BusinessId == businessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, notAvailableTimes, TimeSpan.FromMinutes(10));

            return notAvailableTimes;
        }

        public async Task<IEnumerable<NotAvailableTime>> GetAllByStaffIdAsync(int businessId, int staffId)
        {
            string cacheKey = $"not_available_times_business_{businessId}_staff_{staffId}";
            var cachedTimes = await _redisHelper.GetCacheAsync<List<NotAvailableTime>>(cacheKey);

            if (cachedTimes != null && cachedTimes.Any())
            {
                return cachedTimes;
            }

            var notAvailableTimes = await _context.NotAvailableTimes
                .Where(nat => nat.BusinessId == businessId && nat.StaffId == staffId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, notAvailableTimes, TimeSpan.FromMinutes(10));

            return notAvailableTimes;
        }

        public async Task<NotAvailableTime?> GetByIdAsync(int businessId, int staffId, int id)
        {
            string cacheKey = $"not_available_time_{businessId}_{staffId}_{id}";
            var cachedTime = await _redisHelper.GetCacheAsync<NotAvailableTime>(cacheKey);

            if (cachedTime != null)
            {
                return cachedTime;
            }

            var notAvailableTime = await _context.NotAvailableTimes
                .FirstOrDefaultAsync(nat =>
                    nat.BusinessId == businessId && nat.StaffId == staffId && nat.NotAvailableTimeId == id);

            if (notAvailableTime != null)
            {
                await _redisHelper.SetCacheAsync(cacheKey, notAvailableTime, TimeSpan.FromMinutes(10));
            }

            return notAvailableTime;
        }

        public async Task AddAsync(NotAvailableTime notAvailableTime)
        {
            await _context.NotAvailableTimes.AddAsync(notAvailableTime);
            await _context.SaveChangesAsync();

            // Check the cache for the staff's not available times
            string staffCacheKey =
                $"not_available_times_business_{notAvailableTime.BusinessId}_staff_{notAvailableTime.StaffId}";
            var cachedStaffTimes = await _redisHelper.GetCacheAsync<List<NotAvailableTime>>(staffCacheKey);

            if (cachedStaffTimes == null)
            {
                // If the cache is empty or expired, refresh the cache from the database
                await RefreshRelatedCachesAsync(notAvailableTime);
            }
            else
            {
                // If the cache exists, update it with the new time
                await UpdateNotAvailableTimeCacheAsync(notAvailableTime);
            }
        }

        public async Task UpdateAsync(NotAvailableTime notAvailableTime)
        {
            _context.NotAvailableTimes.Update(notAvailableTime);
            await _context.SaveChangesAsync();

            // Check the cache for the staff's not available times
            string staffCacheKey =
                $"not_available_times_business_{notAvailableTime.BusinessId}_staff_{notAvailableTime.StaffId}";
            var cachedStaffTimes = await _redisHelper.GetCacheAsync<List<NotAvailableTime>>(staffCacheKey);

            if (cachedStaffTimes == null)
            {
                // If the cache is empty or expired, refresh the cache from the database
                await RefreshRelatedCachesAsync(notAvailableTime);
            }
            else
            {
                // If the cache exists, update it with the modified time
                await UpdateNotAvailableTimeCacheAsync(notAvailableTime);
            }
        }

        private async Task UpdateNotAvailableTimeCacheAsync(NotAvailableTime notAvailableTime)
        {
            var timeCacheKey =
                $"not_available_time_{notAvailableTime.BusinessId}_{notAvailableTime.StaffId}_{notAvailableTime.NotAvailableTimeId}";
            await _redisHelper.SetCacheAsync(timeCacheKey, notAvailableTime, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<NotAvailableTime>(
                $"not_available_times_business_{notAvailableTime.BusinessId}_staff_{notAvailableTime.StaffId}",
                list =>
                {
                    // Remove the old entry and add the updated one
                    list.RemoveAll(nat => nat.NotAvailableTimeId == notAvailableTime.NotAvailableTimeId);
                    list.Add(notAvailableTime);
                    return list.OrderBy(nat => nat.NotAvailableTimeId).ToList();
                },
                TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<NotAvailableTime>(
                $"not_available_times_business_{notAvailableTime.BusinessId}",
                list =>
                {
                    list.RemoveAll(nat => nat.NotAvailableTimeId == notAvailableTime.NotAvailableTimeId);
                    list.Add(notAvailableTime);
                    return list.OrderBy(nat => nat.NotAvailableTimeId).ToList();
                },
                TimeSpan.FromMinutes(10));
        }


        public async Task DeleteAsync(int businessId, int staffId, int id)
        {
            var notAvailableTime = await GetByIdAsync(businessId, staffId, id);
            if (notAvailableTime != null)
            {
                _context.NotAvailableTimes.Remove(notAvailableTime);
                await _context.SaveChangesAsync();
                await InvalidateNotAvailableTimeCacheAsync(notAvailableTime);
                await RefreshRelatedCachesAsync(notAvailableTime);
            }
        }

        private async Task InvalidateNotAvailableTimeCacheAsync(NotAvailableTime notAvailableTime)
        {
            var cacheKey =
                $"not_available_time_{notAvailableTime.BusinessId}_{notAvailableTime.StaffId}_{notAvailableTime.NotAvailableTimeId}";
            await _redisHelper.DeleteCacheAsync(cacheKey);

            await _redisHelper.RemoveFromListCacheAsync<NotAvailableTime>(
                $"not_available_times_business_{notAvailableTime.BusinessId}_staff_{notAvailableTime.StaffId}",
                list =>
                {
                    list.RemoveAll(nat => nat.NotAvailableTimeId == notAvailableTime.NotAvailableTimeId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(NotAvailableTime notAvailableTime)
        {
            var cacheKey =
                $"not_available_time_{notAvailableTime.BusinessId}_{notAvailableTime.StaffId}_{notAvailableTime.NotAvailableTimeId}";
            await _redisHelper.SetCacheAsync(cacheKey, notAvailableTime, TimeSpan.FromMinutes(10));

            var staffTimesCacheKey =
                $"not_available_times_business_{notAvailableTime.BusinessId}_staff_{notAvailableTime.StaffId}";
            var staffTimes = await _context.NotAvailableTimes
                .Where(nat => nat.BusinessId == notAvailableTime.BusinessId && nat.StaffId == notAvailableTime.StaffId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(staffTimesCacheKey, staffTimes, TimeSpan.FromMinutes(10));

            var businessTimesCacheKey = $"not_available_times_business_{notAvailableTime.BusinessId}";
            var businessTimes = await _context.NotAvailableTimes
                .Where(nat => nat.BusinessId == notAvailableTime.BusinessId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(businessTimesCacheKey, businessTimes, TimeSpan.FromMinutes(10));
        }
    }
}