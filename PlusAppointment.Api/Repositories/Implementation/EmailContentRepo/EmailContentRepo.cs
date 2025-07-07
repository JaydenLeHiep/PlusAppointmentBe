using PlusAppointment.Models.Classes.Emails;
using PlusAppointment.Repositories.Interfaces.EmailContentRepo;
using PlusAppointment.Data;
using Microsoft.EntityFrameworkCore;

namespace PlusAppointment.Repositories.Implementation.EmailContentRepo;

internal class EmailContentRepo : IEmailContentRepo
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public EmailContentRepo(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<EmailContent>> GetAllAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        var emailContents = await context.EmailContents.ToListAsync();

        return emailContents;
    }

    public async Task<EmailContent?> GetByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var emailContent = await context.EmailContents.FindAsync(id);
        return emailContent;
    }

    public async Task AddAsync(EmailContent emailContent)
    {
        using var context = _contextFactory.CreateDbContext();
        await context.EmailContents.AddAsync(emailContent);
        await context.SaveChangesAsync();
    }

    public async Task AddMultipleAsync(IEnumerable<EmailContent> emailContents)
    {
        using var context = _contextFactory.CreateDbContext();
        await context.EmailContents.AddRangeAsync(emailContents);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmailContent emailContent)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailContents.Update(emailContent);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var emailContent = await context.EmailContents.FindAsync(id);
        if (emailContent != null)
        {
            context.EmailContents.Remove(emailContent);
            await context.SaveChangesAsync();
        }
    }
}
