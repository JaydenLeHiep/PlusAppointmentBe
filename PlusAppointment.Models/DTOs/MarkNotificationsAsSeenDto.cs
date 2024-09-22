namespace PlusAppointment.Models.DTOs;

public class MarkNotificationsAsSeenDto
{
    public int BusinessId { get; set; } // ID of the business
    public List<int> NotificationIds { get; set; } = new List<int>(); // List of notification IDs to mark as seen

}