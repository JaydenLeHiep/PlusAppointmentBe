using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Repositories.Interfaces.DiscountTierRepo;

public interface IDiscountTierRepository
{
    Task<DiscountTier?> GetDiscountTierByIdAsync(int id);
    Task<IEnumerable<DiscountTier>> GetDiscountTiersByBusinessIdAsync(int businessId);
    Task AddDiscountTierAsync(DiscountTier discountTier);
    Task UpdateDiscountTierAsync(DiscountTier discountTier);
    Task DeleteDiscountTierAsync(int id);
}