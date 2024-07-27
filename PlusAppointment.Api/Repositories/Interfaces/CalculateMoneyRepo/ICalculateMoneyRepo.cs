namespace PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;

public interface ICalculateMoneyRepo
{
    Task<decimal> CalculateDailyEarningsAsync(int staffId, decimal commission);
    Task<decimal> CalculateWeeklyEarningsAsync(int staffId, decimal commission);
    Task<decimal> CalculateMonthlyEarningsAsync(int staffId, decimal commission);
    Task<decimal> CalculateDailyEarningsForSpecificDateAsync(int staffId, DateTime specificDate, decimal commission);
    Task InvalidateEarningsCacheAsync(int staffId);
}