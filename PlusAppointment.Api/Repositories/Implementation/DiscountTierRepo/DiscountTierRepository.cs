using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;

using PlusAppointment.Repositories.Interfaces.DiscountTierRepo;

using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Repositories.Implementation.DiscountTierRepo
{
    public class DiscountTierRepository : IDiscountTierRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DiscountTierRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<DiscountTier?> GetDiscountTierByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.DiscountTiers.FindAsync(id);
        }

        public async Task<IEnumerable<DiscountTier>> GetDiscountTiersByBusinessIdAsync(int businessId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.DiscountTiers
                .Where(dt => dt.BusinessId == businessId)
                .OrderBy(dt => dt.CheckInThreshold)
                .ToListAsync();
        }

        public async Task AddDiscountTierAsync(DiscountTier discountTier)
        {
            using var context = _contextFactory.CreateDbContext();
            context.DiscountTiers.Add(discountTier);
            await context.SaveChangesAsync();
        }

        public async Task UpdateDiscountTierAsync(DiscountTier discountTier)
        {
            using var context = _contextFactory.CreateDbContext();
            context.DiscountTiers.Update(discountTier);
            await context.SaveChangesAsync();
        }

        public async Task DeleteDiscountTierAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var discountTier = await context.DiscountTiers.FindAsync(id);
            if (discountTier != null)
            {
                context.DiscountTiers.Remove(discountTier);
                await context.SaveChangesAsync();
            }
        }
    }
}
