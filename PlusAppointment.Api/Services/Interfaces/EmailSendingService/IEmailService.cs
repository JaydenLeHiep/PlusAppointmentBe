namespace PlusAppointment.Services.Interfaces.EmailSendingService;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string? subject, string? body);
    Task<bool> SendBulkEmailAsync(List<string?> toEmails, string? subject, string? body);
    Task<bool> SendBirthdayEmailAsync(string toEmail, string? name, string businessName, decimal discountPercentage, string? discountCode);
}