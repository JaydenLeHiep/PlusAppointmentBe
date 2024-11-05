using PlusAppointment.Models.Classes.CheckIn;

namespace PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;

public interface IDiscountCodeRepository
{
    Task<DiscountCode?> VerifyAndUseDiscountCodeAsync(string code);
    Task AddDiscountCodeAsync(DiscountCode discountCode);
}