namespace PlusAppointment.Models.Classes.Emails;

public class EmailContent
{
    public int EmailContentId { get; set; } // Primary key
    public string Subject { get; set; } = string.Empty; // Email subject
    public string Body { get; set; } = string.Empty; // Email body
}