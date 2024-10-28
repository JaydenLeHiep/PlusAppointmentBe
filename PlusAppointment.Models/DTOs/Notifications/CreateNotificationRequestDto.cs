using PlusAppointment.Models.Classes;

namespace PlusAppointment.Models.DTOs.Notifications;

public class CreateNotificationRequestDto
{
    public int BusinessId { get; set; }
    public string Message { get; set; }
    public NotificationType NotificationType { get; set; }
}