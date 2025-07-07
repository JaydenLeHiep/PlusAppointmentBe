using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;

namespace PlusAppointment.Repositories.Implementation.NotificationRepo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public NotificationRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                await context.Notifications.AddAsync(notification);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByBusinessIdAsync(int businessId)
        {
            var startOfToday = DateTime.UtcNow.Date;
            var endOfToday = startOfToday.AddDays(1).AddTicks(-1);

            using (var context = _contextFactory.CreateDbContext())
            {
                var businessNotifications = await context.Notifications
                    .Where(n => n.BusinessId == businessId && n.CreatedAt >= startOfToday && n.CreatedAt <= endOfToday)
                    .OrderBy(n => n.CreatedAt)
                    .ToListAsync();
                return businessNotifications;
            }
        }
        
        public async Task MarkNotificationsAsSeenAsync(int businessId, List<int> notificationIds)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var notifications = await context.Notifications
                    .Where(n => n.BusinessId == businessId && notificationIds.Contains(n.NotificationId))
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsSeen = true;
                }

                context.Notifications.UpdateRange(notifications);
                await context.SaveChangesAsync();
            }
        }
        
    }
}
