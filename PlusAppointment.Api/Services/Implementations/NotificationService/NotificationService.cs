using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Services.Interfaces.NotificationService;

namespace PlusAppointment.Services.Implementations.NotificationService;

public class NotificationService: INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task AddNotificationAsync(int businessId, string message, NotificationType notificationType)
    {
        var notification = new Notification
        {
            BusinessId = businessId,
            Message = message,
            NotificationType = notificationType,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddNotificationAsync(notification);
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByBusinessIdAsync(int businessId)
    {
        return await _notificationRepository.GetNotificationsByBusinessIdAsync(businessId);
    }
}