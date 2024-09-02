using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.EmailUsageRepo;
using PlusAppointment.Services.Interfaces.EmailUsageService;

namespace PlusAppointment.Services.Implementations.EmailUsageService;

public class EmailUsageService : IEmailUsageService
{
    private readonly IEmailUsageRepo _emailUsageRepo;

    public EmailUsageService(IEmailUsageRepo emailUsageRepo)
    {
        _emailUsageRepo = emailUsageRepo;
    }

    public async Task<EmailUsage?> GetEmailUsageByIdAsync(int emailUsageId)
    {
        return await _emailUsageRepo.GetByIdAsync(emailUsageId);
    }

    public async Task<IEnumerable<EmailUsage>> GetAllEmailUsagesAsync()
    {
        return await _emailUsageRepo.GetAllAsync();
    }

    public async Task<IEnumerable<EmailUsage>> GetEmailUsagesByBusinessIdAndMonthAsync(int businessId, int year, int month)
    {
        return await _emailUsageRepo.GetByBusinessIdAndMonthAsync(businessId, year, month);
    }


    public async Task AddEmailUsageAsync(EmailUsage emailUsage)
    {
        await _emailUsageRepo.AddWithBusinessIdAsync(emailUsage);
    }

    public async Task UpdateEmailUsageAsync(EmailUsage emailUsage)
    {
        await _emailUsageRepo.UpdateWithBusinessIdAsync(emailUsage);
    }

    public async Task DeleteEmailUsageAsync(int emailUsageId)
    {
        await _emailUsageRepo.DeleteWithBusinessIdAsync(emailUsageId);
    }
}