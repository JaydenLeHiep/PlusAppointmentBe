using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.EmailUsageService;

public interface IEmailUsageService
{
    Task<EmailUsage?> GetEmailUsageByIdAsync(int emailUsageId);
    Task<IEnumerable<EmailUsage>> GetAllEmailUsagesAsync();
    Task<IEnumerable<EmailUsage>> GetEmailUsagesByBusinessIdAndMonthAsync(int businessId, int year, int month);
    
    Task AddEmailUsageAsync(EmailUsage emailUsage);
    Task UpdateEmailUsageAsync(EmailUsage emailUsage);
    Task DeleteEmailUsageAsync(int emailUsageId);
}