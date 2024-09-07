namespace PlusAppointment.Utils.SendingEmail;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body);
}