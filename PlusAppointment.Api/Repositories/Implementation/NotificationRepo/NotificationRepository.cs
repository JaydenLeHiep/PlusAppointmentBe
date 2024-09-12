using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;

namespace PlusAppointment.Repositories.Implementation.NotificationRepo;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    // Use the DbContextFactory to create new instances of DbContext
    public NotificationRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        // Create a new DbContext instance
        using (var context = _contextFactory.CreateDbContext())
        {
            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByBusinessIdAsync(int businessId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            // Calculate the start and end of today
            var startOfToday = DateTime.UtcNow.Date; // 00:00 UTC of today
            var endOfToday = startOfToday.AddDays(1).AddTicks(-1); // 23:59:59 UTC of today

            var query = context.Notifications
                .Where(n => n.BusinessId == businessId && n.CreatedAt >= startOfToday && n.CreatedAt <= endOfToday);

            return await query.ToListAsync();
        }
    }

}