using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.NotificationRepo;

public interface INotificationRepository
{
    Task AddNotificationAsync(Notification notification);
    Task<IEnumerable<Notification>> GetNotificationsByBusinessIdAsync(int businessId);

}