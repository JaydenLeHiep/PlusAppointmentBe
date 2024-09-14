using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotAvailableDateRepository;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.NotAvailableDateRepository
{
    public class NotAvailableDateRepository : INotAvailableDateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public NotAvailableDateRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<NotAvailableDate>> GetAllByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"not_available_dates_business_{businessId}";
            var cachedDates = await _redisHelper.GetCacheAsync<List<NotAvailableDate>>(cacheKey);

            if (cachedDates != null && cachedDates.Any())
            {
                return cachedDates;
            }

            var notAvailableDates = await _context.NotAvailableDates
                .Where(nad => nad.BusinessId == businessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, notAvailableDates, TimeSpan.FromMinutes(10));

            return notAvailableDates;
        }

        public async Task<IEnumerable<NotAvailableDate>> GetAllByStaffIdAsync(int businessId, int staffId)
        {
            string cacheKey = $"not_available_dates_business_{businessId}_staff_{staffId}";
            var cachedDates = await _redisHelper.GetCacheAsync<List<NotAvailableDate>>(cacheKey);

            if (cachedDates != null && cachedDates.Any())
            {
                return cachedDates;
            }

            var notAvailableDates = await _context.NotAvailableDates
                .Where(nad => nad.BusinessId == businessId && nad.StaffId == staffId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, notAvailableDates, TimeSpan.FromMinutes(10));

            return notAvailableDates;
        }

        public async Task<NotAvailableDate?> GetByIdAsync(int businessId, int staffId, int id)
        {
            string cacheKey = $"not_available_date_{businessId}_{staffId}_{id}";
            var cachedDate = await _redisHelper.GetCacheAsync<NotAvailableDate>(cacheKey);

            if (cachedDate != null)
            {
                return cachedDate;
            }

            var notAvailableDate = await _context.NotAvailableDates
                .FirstOrDefaultAsync(nad =>
                    nad.BusinessId == businessId && nad.StaffId == staffId && nad.NotAvailableDateId == id);

            if (notAvailableDate != null)
            {
                await _redisHelper.SetCacheAsync(cacheKey, notAvailableDate, TimeSpan.FromMinutes(10));
            }

            return notAvailableDate;
        }

        public async Task AddAsync(NotAvailableDate notAvailableDate)
        {
            await _context.NotAvailableDates.AddAsync(notAvailableDate);
            await _context.SaveChangesAsync();

            // Check the cache for the staff's not available dates
            string staffCacheKey =
                $"not_available_dates_business_{notAvailableDate.BusinessId}_staff_{notAvailableDate.StaffId}";
            var cachedStaffDates = await _redisHelper.GetCacheAsync<List<NotAvailableDate>>(staffCacheKey);

            if (cachedStaffDates == null)
            {
                // If the cache is empty or expired, refresh the cache from the database
                await RefreshRelatedCachesAsync(notAvailableDate);
            }
            else
            {
                // If the cache exists, update it with the new date
                await UpdateNotAvailableDateCacheAsync(notAvailableDate);
            }
        }

        public async Task UpdateAsync(NotAvailableDate notAvailableDate)
        {
            _context.NotAvailableDates.Update(notAvailableDate);
            await _context.SaveChangesAsync();

            // Check the cache for the staff's not available dates
            string staffCacheKey =
                $"not_available_dates_business_{notAvailableDate.BusinessId}_staff_{notAvailableDate.StaffId}";
            var cachedStaffDates = await _redisHelper.GetCacheAsync<List<NotAvailableDate>>(staffCacheKey);

            if (cachedStaffDates == null)
            {
                // If the cache is empty or expired, refresh the cache from the database
                await RefreshRelatedCachesAsync(notAvailableDate);
            }
            else
            {
                // If the cache exists, update it with the modified date
                await UpdateNotAvailableDateCacheAsync(notAvailableDate);
            }
        }

        private async Task UpdateNotAvailableDateCacheAsync(NotAvailableDate notAvailableDate)
        {
            var dateCacheKey =
                $"not_available_date_{notAvailableDate.BusinessId}_{notAvailableDate.StaffId}_{notAvailableDate.NotAvailableDateId}";
            await _redisHelper.SetCacheAsync(dateCacheKey, notAvailableDate, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<NotAvailableDate>(
                $"not_available_dates_business_{notAvailableDate.BusinessId}_staff_{notAvailableDate.StaffId}",
                list =>
                {
                    // Remove the old entry and add the updated one
                    list.RemoveAll(nad => nad.NotAvailableDateId == notAvailableDate.NotAvailableDateId);
                    list.Add(notAvailableDate);
                    return list.OrderBy(nad => nad.NotAvailableDateId).ToList();
                },
                TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<NotAvailableDate>(
                $"not_available_dates_business_{notAvailableDate.BusinessId}",
                list =>
                {
                    list.RemoveAll(nad => nad.NotAvailableDateId == notAvailableDate.NotAvailableDateId);
                    list.Add(notAvailableDate);
                    return list.OrderBy(nad => nad.NotAvailableDateId).ToList();
                },
                TimeSpan.FromMinutes(10));
        }


        public async Task DeleteAsync(int businessId, int staffId, int id)
        {
            var notAvailableDate = await GetByIdAsync(businessId, staffId, id);
            if (notAvailableDate != null)
            {
                _context.NotAvailableDates.Remove(notAvailableDate);
                await _context.SaveChangesAsync();
                await InvalidateNotAvailableDateCacheAsync(notAvailableDate);
                await RefreshRelatedCachesAsync(notAvailableDate);
            }
        }

        private async Task InvalidateNotAvailableDateCacheAsync(NotAvailableDate notAvailableDate)
        {
            var cacheKey =
                $"not_available_date_{notAvailableDate.BusinessId}_{notAvailableDate.StaffId}_{notAvailableDate.NotAvailableDateId}";
            await _redisHelper.DeleteCacheAsync(cacheKey);

            await _redisHelper.RemoveFromListCacheAsync<NotAvailableDate>(
                $"not_available_dates_business_{notAvailableDate.BusinessId}_staff_{notAvailableDate.StaffId}",
                list =>
                {
                    list.RemoveAll(nad => nad.NotAvailableDateId == notAvailableDate.NotAvailableDateId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(NotAvailableDate notAvailableDate)
        {
            var cacheKey =
                $"not_available_date_{notAvailableDate.BusinessId}_{notAvailableDate.StaffId}_{notAvailableDate.NotAvailableDateId}";
            await _redisHelper.SetCacheAsync(cacheKey, notAvailableDate, TimeSpan.FromMinutes(10));

            var staffDatesCacheKey =
                $"not_available_dates_business_{notAvailableDate.BusinessId}_staff_{notAvailableDate.StaffId}";
            var staffDates = await _context.NotAvailableDates
                .Where(nad => nad.BusinessId == notAvailableDate.BusinessId && nad.StaffId == notAvailableDate.StaffId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(staffDatesCacheKey, staffDates, TimeSpan.FromMinutes(10));

            var businessDatesCacheKey = $"not_available_dates_business_{notAvailableDate.BusinessId}";
            var businessDates = await _context.NotAvailableDates
                .Where(nad => nad.BusinessId == notAvailableDate.BusinessId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(businessDatesCacheKey, businessDates, TimeSpan.FromMinutes(10));
        }
    }
}