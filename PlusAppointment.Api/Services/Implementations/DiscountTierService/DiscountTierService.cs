using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Models.DTOs.CheckIn;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.DiscountTierRepo;
using PlusAppointment.Services.Interfaces.DiscountTierService;

namespace PlusAppointment.Services.Implementations.DiscountTierService;

public class DiscountTierService : IDiscountTierService
{
    private readonly IDiscountTierRepository _discountTierRepository;
    private readonly IBusinessRepository _businessRepository;

    public DiscountTierService(IDiscountTierRepository discountTierRepository, IBusinessRepository businessRepository)
    {
        _discountTierRepository = discountTierRepository;
        _businessRepository = businessRepository;
    }

    public async Task<DiscountTier?> GetDiscountTierByIdAsync(int id) =>
        await _discountTierRepository.GetDiscountTierByIdAsync(id);

    public async Task<IEnumerable<DiscountTier>> GetDiscountTiersByBusinessIdAsync(int businessId) =>
        await _discountTierRepository.GetDiscountTiersByBusinessIdAsync(businessId);

    public async Task AddDiscountTierAsync(DiscountTierDto discountTierDto, int businessId)
    {
        

        var business = await _businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new KeyNotFoundException("Business not found.");
        }

        var discountTier = new DiscountTier
        {
            BusinessId = businessId,
            CheckInThreshold = discountTierDto.CheckInThreshold,
            DiscountPercentage = discountTierDto.DiscountPercentage
        };

        await _discountTierRepository.AddDiscountTierAsync(discountTier);
    }

    

    public async Task UpdateDiscountTierAsync(int id, DiscountTierDto discountTierDto)
    {
        var existingTier = await _discountTierRepository.GetDiscountTierByIdAsync(id);
        if (existingTier == null)
        {
            throw new KeyNotFoundException("Discount tier not found.");
        }

        existingTier.CheckInThreshold = discountTierDto.CheckInThreshold;
        existingTier.DiscountPercentage = discountTierDto.DiscountPercentage;
        await _discountTierRepository.UpdateDiscountTierAsync(existingTier);
    }

    public async Task DeleteDiscountTierAsync(int id) =>
        await _discountTierRepository.DeleteDiscountTierAsync(id);
}