using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.NotificationService;

public interface INotificationService
{
    Task AddNotificationAsync(int businessId, string message, NotificationType notificationType);
    Task<IEnumerable<Notification>> GetNotificationsByBusinessIdAsync(int businessId);
    Task MarkNotificationsAsSeenAsync(int businessId, List<int> notificationIds);

}