using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Services.Interfaces.CalculateMoneyService;

using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Implementations.CalculateMoneyService
{
    public class CalculateMoneyService : ICalculateMoneyService
    {
        private readonly ICalculateMoneyRepo _calculateMoneyRepo;

        public CalculateMoneyService(ICalculateMoneyRepo calculateMoneyRepo)
        {
            _calculateMoneyRepo = calculateMoneyRepo;
        }

        public async Task<EarningsResult> GetDailyEarningsAsync(int staffId, decimal commission)
        {
            var earnings = await _calculateMoneyRepo.CalculateDailyEarningsAsync(staffId, commission);
            return CalculateEarningsResult(earnings);
        }

        public async Task<EarningsResult> GetWeeklyEarningsAsync(int staffId, decimal commission)
        {
            var earnings = await _calculateMoneyRepo.CalculateWeeklyEarningsAsync(staffId, commission);
            return CalculateEarningsResult(earnings);
        }

        public async Task<EarningsResult> GetMonthlyEarningsAsync(int staffId, decimal commission)
        {
            var earnings = await _calculateMoneyRepo.CalculateMonthlyEarningsAsync(staffId, commission);
            return CalculateEarningsResult(earnings);
        }

        public async Task<EarningsResult> GetDailyEarningsForSpecificDateAsync(int staffId, decimal commission, DateTime specificDate)
        {
            var earnings = await _calculateMoneyRepo.CalculateDailyEarningsForSpecificDateAsync(staffId, specificDate, commission);
            return CalculateEarningsResult(earnings);
        }

        private EarningsResult CalculateEarningsResult(decimal earnings)
        {
            if (earnings == 0)
            {
                return new EarningsResult
                {
                    Success = false,
                    ErrorMessage = "No earnings found for the specified period."
                };
            }

            return new EarningsResult
            {
                Success = true,
                Earnings = earnings
            };
        }
    }
}
