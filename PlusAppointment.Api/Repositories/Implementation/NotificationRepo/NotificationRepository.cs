using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.NotificationRepo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly RedisHelper _redisHelper;

        public NotificationRepository(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
        {
            _contextFactory = contextFactory;
            _redisHelper = redisHelper;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            // Add the notification to the database
            using (var context = _contextFactory.CreateDbContext())
            {
                await context.Notifications.AddAsync(notification);
                await context.SaveChangesAsync();
            }

            // Check the cache and update it after adding the new notification
            string cacheKey = $"notifications_business_{notification.BusinessId}";
            var cachedNotifications = await _redisHelper.GetCacheAsync<List<Notification>>(cacheKey);

            if (cachedNotifications == null)
            {
                // If the cache is empty or expired, fetch notifications from the database
                await RefreshRelatedCachesAsync(notification.BusinessId);
            }
            else
            {
                // If the cache exists, just update it
                await UpdateNotificationCacheAsync(notification);
            }
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"notifications_business_{businessId}";
            var cachedNotifications = await _redisHelper.GetCacheAsync<List<Notification>>(cacheKey);

            if (cachedNotifications != null && cachedNotifications.Any())
            {
                return cachedNotifications.OrderBy(n => n.CreatedAt);
            }

            // If the cache is not present, fetch from the database and set the cache
            await RefreshRelatedCachesAsync(businessId);
            return await _redisHelper.GetCacheAsync<List<Notification>>(cacheKey) ?? new List<Notification>();
        }

        private async Task UpdateNotificationCacheAsync(Notification notification)
        {
            string cacheKey = $"notifications_business_{notification.BusinessId}";

            await _redisHelper.UpdateListCacheAsync<Notification>(
                cacheKey,
                list =>
                {
                    // Add the new notification to the list
                    list.Add(notification);
                    return list.OrderBy(n => n.CreatedAt).ToList();
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(int businessId)
        {
            string cacheKey = $"notifications_business_{businessId}";
            var startOfToday = DateTime.UtcNow.Date;
            var endOfToday = startOfToday.AddDays(1).AddTicks(-1);

            using (var context = _contextFactory.CreateDbContext())
            {
                var businessNotifications = await context.Notifications
                    .Where(n => n.BusinessId == businessId && n.CreatedAt >= startOfToday && n.CreatedAt <= endOfToday)
                    .OrderBy(n => n.CreatedAt)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(cacheKey, businessNotifications, TimeSpan.FromMinutes(10));
            }
        }

        private async Task InvalidateNotificationCacheAsync(int businessId)
        {
            string cacheKey = $"notifications_business_{businessId}";
            await _redisHelper.DeleteCacheAsync(cacheKey);
        }
    }
}
