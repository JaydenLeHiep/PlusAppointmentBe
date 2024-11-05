using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Services.Interfaces.DiscountCodeService;

public interface IDiscountCodeService
{
    Task<DiscountCode?> VerifyAndUseDiscountCodeAsync(string code);
    Task AddDiscountCodeAsync(DiscountCode discountCode);
}