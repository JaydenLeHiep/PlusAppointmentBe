using PlusAppointment.Models.Classes.Emails;

namespace PlusAppointment.Services.Interfaces.EmailContentService;

public interface IEmailContentService
{
    Task<IEnumerable<EmailContent>> GetAllAsync();
    Task<EmailContent?> GetByIdAsync(int id);
    Task AddAsync(EmailContent emailContent);
    Task AddMultipleAsync(IEnumerable<EmailContent> emailContents);
    Task UpdateAsync(EmailContent emailContent);
    Task DeleteAsync(int id);
}