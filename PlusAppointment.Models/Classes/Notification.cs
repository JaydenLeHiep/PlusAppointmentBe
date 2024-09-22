namespace PlusAppointment.Models.Classes;

public class Notification
{
    public int NotificationId { get; set; } // Primary Key
    public int BusinessId { get; set; } // Foreign Key to Business
    public Business? Business { get; set; } // Navigation property

    public string Message { get; set; } = string.Empty; // Message for the notification
    public NotificationType NotificationType { get; set; } // Type: Add, Cancel, Update
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp of creation
    public bool IsSeen { get; set; } = false;
}

public enum NotificationType
{
    Add,
    Cancel,
    Update
}