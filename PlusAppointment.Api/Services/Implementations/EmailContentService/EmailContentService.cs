using PlusAppointment.Models.Classes.Emails;
using PlusAppointment.Repositories.Interfaces.EmailContentRepo;
using PlusAppointment.Services.Interfaces.EmailContentService;

namespace PlusAppointment.Services.Implementations.EmailContentService;

public class EmailContentService : IEmailContentService
{
    private readonly IEmailContentRepo _emailContentRepo;

    public EmailContentService(IEmailContentRepo emailContentRepo)
    {
        _emailContentRepo = emailContentRepo;
    }

    public async Task<IEnumerable<EmailContent>> GetAllAsync()
    {
        return await _emailContentRepo.GetAllAsync();
    }

    public async Task<EmailContent?> GetByIdAsync(int id)
    {
        return await _emailContentRepo.GetByIdAsync(id);
    }

    public async Task AddAsync(EmailContent emailContent)
    {
        await _emailContentRepo.AddAsync(emailContent);
    }

    public async Task AddMultipleAsync(IEnumerable<EmailContent> emailContents)
    {
        await _emailContentRepo.AddMultipleAsync(emailContents);
    }

    public async Task UpdateAsync(EmailContent emailContent)
    {
        await _emailContentRepo.UpdateAsync(emailContent);
    }

    public async Task DeleteAsync(int id)
    {
        await _emailContentRepo.DeleteAsync(id);
    }
}