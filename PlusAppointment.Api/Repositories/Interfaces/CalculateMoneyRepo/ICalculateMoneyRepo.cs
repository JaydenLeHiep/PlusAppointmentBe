namespace PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;

public interface ICalculateMoneyRepo
{
    Task<decimal> CalculateDailyEarningsAsync(int staffId);
    Task<decimal> CalculateWeeklyEarningsAsync(int staffId);
    Task<decimal> CalculateMonthlyEarningsAsync(int staffId);
    Task<decimal> CalculateDailyEarningsForSpecificDateAsync(int staffId, DateTime specificDate);
    Task InvalidateEarningsCacheAsync(int staffId);
}