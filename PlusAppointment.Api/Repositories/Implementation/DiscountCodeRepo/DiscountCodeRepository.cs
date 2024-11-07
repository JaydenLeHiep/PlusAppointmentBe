using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;

namespace PlusAppointment.Repositories.Implementation.DiscountCodeRepo;

public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public DiscountCodeRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddDiscountCodeAsync(DiscountCode discountCode)
    {
        using var context = _contextFactory.CreateDbContext();
        context.DiscountCodes.Add(discountCode);
        await context.SaveChangesAsync();
    }

    public async Task<DiscountCode?> VerifyAndUseDiscountCodeAsync(string code)
    {
        using var context = _contextFactory.CreateDbContext();
        var discountCode = await context.DiscountCodes.FirstOrDefaultAsync(dc => dc.Code == code && !dc.IsUsed);

        if (discountCode == null)
        {
            // Code is either invalid or already used
            return null;
        }

        // Mark as used
        discountCode.IsUsed = true;
        await context.SaveChangesAsync();

        return discountCode;
    }
}