using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.EmailUsageRepo;

namespace PlusAppointment.Repositories.Implementation.EmailUsageRepo
{
    public class EmailUsageRepo : IEmailUsageRepo
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public EmailUsageRepo(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<EmailUsage?> GetByIdAsync(int emailUsageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsage = await context.EmailUsages.FindAsync(emailUsageId);
                if (emailUsage == null)
                {
                    throw new KeyNotFoundException($"EmailUsage with ID {emailUsageId} not found");
                }
                return emailUsage;
            }
        }


        public async Task<IEnumerable<EmailUsage>> GetAllAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsages = await context.EmailUsages.ToListAsync();

                return emailUsages;
            }
        }

        public async Task<IEnumerable<EmailUsage>> GetByBusinessIdAndMonthAsync(int businessId, int year, int month)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsages = await context.EmailUsages
                    .Where(eu => eu.BusinessId == businessId && eu.Year == year && eu.Month == month)
                    .ToListAsync();
                
                return emailUsages;
            }
        }

        public async Task AddWithBusinessIdAsync(EmailUsage emailUsage)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var existingRecord = await context.EmailUsages
                    .FirstOrDefaultAsync(e => e.BusinessId == emailUsage.BusinessId &&
                                              e.Year == emailUsage.Year &&
                                              e.Month == emailUsage.Month);

                if (existingRecord != null)
                {
                    existingRecord.EmailCount += emailUsage.EmailCount;
                    context.EmailUsages.Update(existingRecord);
                }
                else
                {
                    context.EmailUsages.Add(emailUsage);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateWithBusinessIdAsync(EmailUsage emailUsage)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.EmailUsages.Update(emailUsage);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteWithBusinessIdAsync(int emailUsageId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var emailUsage = await context.EmailUsages.FindAsync(emailUsageId);
                if (emailUsage != null)
                {
                    context.EmailUsages.Remove(emailUsage);
                    await context.SaveChangesAsync();
                }
            }
        }
        
    }
}
