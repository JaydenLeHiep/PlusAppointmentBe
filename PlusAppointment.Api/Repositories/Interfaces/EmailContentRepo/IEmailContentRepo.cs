using PlusAppointment.Models.Classes.Emails;

namespace PlusAppointment.Repositories.Interfaces.EmailContentRepo;

public interface IEmailContentRepo
{
    Task<IEnumerable<EmailContent>> GetAllAsync();
    Task<EmailContent?> GetByIdAsync(int id);
    Task AddAsync(EmailContent emailContent);
    Task AddMultipleAsync(IEnumerable<EmailContent> emailContents); // New method
    Task UpdateAsync(EmailContent emailContent);
    Task DeleteAsync(int id);
}