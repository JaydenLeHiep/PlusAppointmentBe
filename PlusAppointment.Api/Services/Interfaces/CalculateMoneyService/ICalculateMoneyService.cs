namespace PlusAppointment.Services.Interfaces.CalculateMoneyService;

public interface ICalculateMoneyService
{
    Task<decimal> GetDailyEarningsAsync(int staffId, decimal commission);
    Task<decimal> GetWeeklyEarningsAsync(int staffId, decimal commission);
    Task<decimal> GetMonthlyEarningsAsync(int staffId, decimal commission);
    Task<decimal> GetDailyEarningsForSpecificDateAsync(int staffId, decimal commission, DateTime specificDate);

}