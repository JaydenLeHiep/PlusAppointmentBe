using PlusAppointment.Models.Classes;

namespace PlusAppointment.Models.DTOs;

public class CreateNotificationRequestDto
{
    public int BusinessId { get; set; }
    public string Message { get; set; }
    public NotificationType NotificationType { get; set; }
}