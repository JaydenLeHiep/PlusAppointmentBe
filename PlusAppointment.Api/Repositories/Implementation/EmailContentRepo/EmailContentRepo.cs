using PlusAppointment.Models.Classes.Emails;
using PlusAppointment.Repositories.Interfaces.EmailContentRepo;
using PlusAppointment.Data;
using Microsoft.EntityFrameworkCore;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.EmailContentRepo;

internal class EmailContentRepo : IEmailContentRepo
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly RedisHelper _redisHelper;

    public EmailContentRepo(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
    {
        _contextFactory = contextFactory;
        _redisHelper = redisHelper;
    }

    public async Task<IEnumerable<EmailContent>> GetAllAsync()
    {
        const string cacheKey = "all_email_contents";
        var cachedEmailContents = await _redisHelper.GetCacheAsync<List<EmailContent>>(cacheKey);

        if (cachedEmailContents != null && cachedEmailContents.Any())
        {
            return cachedEmailContents;
        }

        using var context = _contextFactory.CreateDbContext();
        var emailContents = await context.EmailContents.ToListAsync();
        await _redisHelper.SetCacheAsync(cacheKey, emailContents, TimeSpan.FromMinutes(10));

        return emailContents;
    }

    public async Task<EmailContent?> GetByIdAsync(int id)
    {
        string cacheKey = $"email_content_{id}";
        var cachedEmailContent = await _redisHelper.GetCacheAsync<EmailContent>(cacheKey);

        if (cachedEmailContent != null)
        {
            return cachedEmailContent;
        }

        using var context = _contextFactory.CreateDbContext();
        var emailContent = await context.EmailContents.FindAsync(id);

        if (emailContent != null)
        {
            await _redisHelper.SetCacheAsync(cacheKey, emailContent, TimeSpan.FromMinutes(10));
        }

        return emailContent;
    }

    public async Task AddAsync(EmailContent emailContent)
    {
        using var context = _contextFactory.CreateDbContext();
        await context.EmailContents.AddAsync(emailContent);
        await context.SaveChangesAsync();
        await RefreshCacheAsync(emailContent);
    }

    public async Task AddMultipleAsync(IEnumerable<EmailContent> emailContents)
    {
        using var context = _contextFactory.CreateDbContext();
        await context.EmailContents.AddRangeAsync(emailContents);
        await context.SaveChangesAsync();
        
        foreach (var emailContent in emailContents)
        {
            await RefreshCacheAsync(emailContent);
        }
    }

    public async Task UpdateAsync(EmailContent emailContent)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailContents.Update(emailContent);
        await context.SaveChangesAsync();
        await RefreshCacheAsync(emailContent);
    }

    public async Task DeleteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var emailContent = await context.EmailContents.FindAsync(id);
        if (emailContent != null)
        {
            context.EmailContents.Remove(emailContent);
            await context.SaveChangesAsync();
            await InvalidateCacheAsync(emailContent);
        }
    }

    private async Task RefreshCacheAsync(EmailContent emailContent)
    {
        var cacheKey = $"email_content_{emailContent.EmailContentId}";
        await _redisHelper.SetCacheAsync(cacheKey, emailContent, TimeSpan.FromMinutes(10));

        var allCacheKey = "all_email_contents";
        using var context = _contextFactory.CreateDbContext();
        var allEmailContents = await context.EmailContents.ToListAsync();
        await _redisHelper.SetCacheAsync(allCacheKey, allEmailContents, TimeSpan.FromMinutes(10));
    }

    private async Task InvalidateCacheAsync(EmailContent emailContent)
    {
        var cacheKey = $"email_content_{emailContent.EmailContentId}";
        await _redisHelper.DeleteCacheAsync(cacheKey);

        const string allCacheKey = "all_email_contents";
        await _redisHelper.DeleteCacheAsync(allCacheKey);
    }
}
