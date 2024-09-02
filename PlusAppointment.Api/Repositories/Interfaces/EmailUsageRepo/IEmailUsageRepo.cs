using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.EmailUsageRepo;

public interface IEmailUsageRepo
{
    Task<EmailUsage?> GetByIdAsync(int emailUsageId);
    Task<IEnumerable<EmailUsage>> GetAllAsync();
    Task<IEnumerable<EmailUsage>> GetByBusinessIdAndMonthAsync(int businessId, int year, int month);
    Task AddWithBusinessIdAsync(EmailUsage emailUsage);
    Task UpdateWithBusinessIdAsync(EmailUsage emailUsage);
    Task DeleteWithBusinessIdAsync(int emailUsageId);
}