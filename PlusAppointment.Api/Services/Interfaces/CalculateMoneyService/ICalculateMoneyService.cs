using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.CalculateMoneyService;

public interface ICalculateMoneyService
{
    Task<EarningsResult> GetDailyEarningsAsync(int staffId, decimal commission);
    Task<EarningsResult> GetWeeklyEarningsAsync(int staffId, decimal commission);
    Task<EarningsResult> GetMonthlyEarningsAsync(int staffId, decimal commission);
    Task<EarningsResult> GetDailyEarningsForSpecificDateAsync(int staffId, decimal commission, DateTime specificDate);

}