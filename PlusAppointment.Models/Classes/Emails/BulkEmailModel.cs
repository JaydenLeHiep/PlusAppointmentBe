namespace PlusAppointment.Models.Classes.Emails;

public class BulkEmailModel
{
    public List<string?> ToEmails { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}