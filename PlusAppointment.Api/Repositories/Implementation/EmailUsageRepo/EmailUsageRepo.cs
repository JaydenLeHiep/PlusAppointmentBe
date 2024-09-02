using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.EmailUsageRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.EmailUsageRepo;

public class EmailUsageRepo : IEmailUsageRepo
{
    private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public EmailUsageRepo(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<EmailUsage?> GetByIdAsync(int emailUsageId)
        {
            string cacheKey = $"email_usage_{emailUsageId}";
            var emailUsage = await _redisHelper.GetCacheAsync<EmailUsage>(cacheKey);

            if (emailUsage != null)
            {
                return emailUsage;
            }

            emailUsage = await _context.EmailUsages.FindAsync(emailUsageId);
            if (emailUsage == null)
            {
                throw new KeyNotFoundException($"EmailUsage with ID {emailUsageId} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, emailUsage, TimeSpan.FromMinutes(10));
            return emailUsage;
        }

        public async Task<IEnumerable<EmailUsage>> GetAllAsync()
        {
            const string cacheKey = "all_email_usages";
            var cachedEmailUsages = await _redisHelper.GetCacheAsync<List<EmailUsage>>(cacheKey);

            if (cachedEmailUsages != null && cachedEmailUsages.Any())
            {
                return cachedEmailUsages;
            }

            var emailUsages = await _context.EmailUsages.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, emailUsages, TimeSpan.FromMinutes(10));

            return emailUsages;
        }

        public async Task<IEnumerable<EmailUsage>> GetByBusinessIdAndMonthAsync(int businessId, int year, int month)
        {
            string cacheKey = $"email_usages_business_{businessId}_year_{year}_month_{month}";
            var cachedEmailUsages = await _redisHelper.GetCacheAsync<List<EmailUsage>>(cacheKey);

            if (cachedEmailUsages != null && cachedEmailUsages.Any())
            {
                return cachedEmailUsages;
            }

            var emailUsages = await _context.EmailUsages
                .Where(eu => eu.BusinessId == businessId && eu.Year == year && eu.Month == month)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, emailUsages, TimeSpan.FromMinutes(10));
            return emailUsages;
        }


        public async Task AddWithBusinessIdAsync(EmailUsage emailUsage)
        {
            // Check if the record with the same business_id, year, and month already exists
            var existingRecord = await _context.EmailUsages
                .FirstOrDefaultAsync(e => e.BusinessId == emailUsage.BusinessId &&
                                          e.Year == emailUsage.Year &&
                                          e.Month == emailUsage.Month);

            if (existingRecord != null)
            {
                // Update the existing record, for example, by incrementing the email count
                existingRecord.EmailCount += emailUsage.EmailCount;
                _context.EmailUsages.Update(existingRecord);
            }
            else
            {
                // Insert a new record if it doesn't exist
                _context.EmailUsages.Add(emailUsage);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Update caches
            await UpdateEmailUsageCacheAsync(emailUsage);
            await RefreshRelatedCachesAsync(emailUsage);
        }


        public async Task UpdateWithBusinessIdAsync(EmailUsage emailUsage)
        {
            _context.EmailUsages.Update(emailUsage);
            await _context.SaveChangesAsync();

            await UpdateEmailUsageCacheAsync(emailUsage);
            await RefreshRelatedCachesAsync(emailUsage);
        }

        public async Task DeleteWithBusinessIdAsync(int emailUsageId)
        {
            var emailUsage = await _context.EmailUsages.FindAsync(emailUsageId);
            if (emailUsage != null)
            {
                _context.EmailUsages.Remove(emailUsage);
                await _context.SaveChangesAsync();

                await InvalidateEmailUsageCacheAsync(emailUsage);
                await RefreshRelatedCachesAsync(emailUsage);
            }
        }

        private async Task UpdateEmailUsageCacheAsync(EmailUsage emailUsage)
        {
            var emailUsageCacheKey = $"email_usage_{emailUsage.EmailUsageId}";
            await _redisHelper.SetCacheAsync(emailUsageCacheKey, emailUsage, TimeSpan.FromMinutes(10));

            var emailUsagesByBusinessCacheKey = $"email_usages_business_{emailUsage.BusinessId}";
            await _redisHelper.UpdateListCacheAsync<EmailUsage>(
                emailUsagesByBusinessCacheKey,
                list =>
                {
                    list.RemoveAll(eu => eu.EmailUsageId == emailUsage.EmailUsageId);
                    list.Add(emailUsage);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateEmailUsageCacheAsync(EmailUsage emailUsage)
        {
            var emailUsageCacheKey = $"email_usage_{emailUsage.EmailUsageId}";
            await _redisHelper.DeleteCacheAsync(emailUsageCacheKey);

            var emailUsagesByBusinessCacheKey = $"email_usages_business_{emailUsage.BusinessId}";
            await _redisHelper.RemoveFromListCacheAsync<EmailUsage>(
                emailUsagesByBusinessCacheKey,
                list =>
                {
                    list.RemoveAll(eu => eu.EmailUsageId == emailUsage.EmailUsageId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(EmailUsage emailUsage)
        {
            // Refresh individual EmailUsage cache
            var emailUsageCacheKey = $"email_usage_{emailUsage.EmailUsageId}";
            await _redisHelper.SetCacheAsync(emailUsageCacheKey, emailUsage, TimeSpan.FromMinutes(10));

            // Refresh list of all EmailUsages
            const string allEmailUsagesCacheKey = "all_email_usages";
            var allEmailUsages = await _context.EmailUsages.ToListAsync();
            await _redisHelper.SetCacheAsync(allEmailUsagesCacheKey, allEmailUsages, TimeSpan.FromMinutes(10));

            // Refresh list of EmailUsages by business ID
            var emailUsagesByBusinessCacheKey = $"email_usages_business_{emailUsage.BusinessId}";
            var emailUsagesByBusiness = await _context.EmailUsages
                .Where(eu => eu.BusinessId == emailUsage.BusinessId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(emailUsagesByBusinessCacheKey, emailUsagesByBusiness, TimeSpan.FromMinutes(10));
        }
}