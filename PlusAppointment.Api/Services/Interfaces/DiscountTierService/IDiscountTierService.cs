using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Models.DTOs.CheckIn;

namespace PlusAppointment.Services.Interfaces.DiscountTierService;

public interface IDiscountTierService
{
    Task<DiscountTier?> GetDiscountTierByIdAsync(int id);
    Task<IEnumerable<DiscountTier>> GetDiscountTiersByBusinessIdAsync(int businessId);
    Task AddDiscountTierAsync(DiscountTierDto discountTierDto, int businessId);
    Task UpdateDiscountTierAsync(int id, DiscountTierDto discountTierDto);
    Task DeleteDiscountTierAsync(int id);
}