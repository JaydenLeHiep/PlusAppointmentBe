using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.EmailUsageRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.EmailUsageRepo
{
    public class EmailUsageRepo : IEmailUsageRepo
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly RedisHelper _redisHelper;

        public EmailUsageRepo(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
        {
            _contextFactory = contextFactory;
            _redisHelper = redisHelper;
        }

        public async Task<EmailUsage?> GetByIdAsync(int emailUsageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsage = await context.EmailUsages.FindAsync(emailUsageId);
                if (emailUsage == null)
                {
                    throw new KeyNotFoundException($"EmailUsage with ID {emailUsageId} not found");
                }
                return emailUsage;
            }
        }


        public async Task<IEnumerable<EmailUsage>> GetAllAsync()
        {
            // const string cacheKey = "all_email_usages";
            // var cachedEmailUsages = await _redisHelper.GetCacheAsync<List<EmailUsage>>(cacheKey);
            //
            // if (cachedEmailUsages != null && cachedEmailUsages.Any())
            // {
            //     return cachedEmailUsages;
            // }

            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsages = await context.EmailUsages.ToListAsync();
                //await _redisHelper.SetCacheAsync(cacheKey, emailUsages, TimeSpan.FromMinutes(10));
                return emailUsages;
            }
        }

        public async Task<IEnumerable<EmailUsage>> GetByBusinessIdAndMonthAsync(int businessId, int year, int month)
        {
            // string cacheKey = $"email_usages_business_{businessId}_year_{year}_month_{month}";
            // var cachedEmailUsages = await _redisHelper.GetCacheAsync<List<EmailUsage>>(cacheKey);
            //
            // if (cachedEmailUsages != null && cachedEmailUsages.Any())
            // {
            //     return cachedEmailUsages;
            // }

            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsages = await context.EmailUsages
                    .Where(eu => eu.BusinessId == businessId && eu.Year == year && eu.Month == month)
                    .ToListAsync();

                //await _redisHelper.SetCacheAsync(cacheKey, emailUsages, TimeSpan.FromMinutes(10));
                return emailUsages;
            }
        }

        public async Task AddWithBusinessIdAsync(EmailUsage emailUsage)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var existingRecord = await context.EmailUsages
                    .FirstOrDefaultAsync(e => e.BusinessId == emailUsage.BusinessId &&
                                              e.Year == emailUsage.Year &&
                                              e.Month == emailUsage.Month);

                if (existingRecord != null)
                {
                    existingRecord.EmailCount += emailUsage.EmailCount;
                    context.EmailUsages.Update(existingRecord);
                }
                else
                {
                    context.EmailUsages.Add(emailUsage);
                }

                await context.SaveChangesAsync();
            }

            // await UpdateEmailUsageCacheAsync(emailUsage);
            // await RefreshRelatedCachesAsync(emailUsage);
        }

        public async Task UpdateWithBusinessIdAsync(EmailUsage emailUsage)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.EmailUsages.Update(emailUsage);
                await context.SaveChangesAsync();
            }

            // await UpdateEmailUsageCacheAsync(emailUsage);
            // await RefreshRelatedCachesAsync(emailUsage);
        }

        public async Task DeleteWithBusinessIdAsync(int emailUsageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsage = await context.EmailUsages.FindAsync(emailUsageId);
                if (emailUsage != null)
                {
                    context.EmailUsages.Remove(emailUsage);
                    await context.SaveChangesAsync();

                    await InvalidateEmailUsageCacheAsync(emailUsage);
                    await RefreshRelatedCachesAsync(emailUsage);
                }
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
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsageCacheKey = $"email_usage_{emailUsage.EmailUsageId}";
                await _redisHelper.SetCacheAsync(emailUsageCacheKey, emailUsage, TimeSpan.FromMinutes(10));

                var emailUsagesByBusinessCacheKey = $"email_usages_business_{emailUsage.BusinessId}";
                var emailUsagesByBusiness = await context.EmailUsages
                    .Where(eu => eu.BusinessId == emailUsage.BusinessId)
                    .ToListAsync();
                await _redisHelper.SetCacheAsync(emailUsagesByBusinessCacheKey, emailUsagesByBusiness, TimeSpan.FromMinutes(10));

                const string allEmailUsagesCacheKey = "all_email_usages";
                var allEmailUsages = await context.EmailUsages.ToListAsync();
                await _redisHelper.SetCacheAsync(allEmailUsagesCacheKey, allEmailUsages, TimeSpan.FromMinutes(10));
            }
        }
    }
}
