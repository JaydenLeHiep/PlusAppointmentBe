using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;
using PlusAppointment.Services.Interfaces.DiscountCodeService;

namespace PlusAppointment.Services.Implementations.DiscountCodeService;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public DiscountCodeService(IDiscountCodeRepository discountCodeRepository)
    {
        _discountCodeRepository = discountCodeRepository;
    }

    public async Task<DiscountCode?> VerifyAndUseDiscountCodeAsync(string code)
    {
        return await _discountCodeRepository.VerifyAndUseDiscountCodeAsync(code);
    }

    public async Task AddDiscountCodeAsync(DiscountCode discountCode)
    {
        await _discountCodeRepository.AddDiscountCodeAsync(discountCode);
    }
}