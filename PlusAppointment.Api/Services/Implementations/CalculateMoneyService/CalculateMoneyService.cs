using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Services.Interfaces.CalculateMoneyService;

namespace PlusAppointment.Services.Implementations.CalculateMoneyService
{
    public class CalculateMoneyService : ICalculateMoneyService
    {
        private readonly ICalculateMoneyRepo _calculateMoneyRepo;

        public CalculateMoneyService(ICalculateMoneyRepo calculateMoneyRepo)
        {
            _calculateMoneyRepo = calculateMoneyRepo;
        }

        public async Task<decimal> GetDailyEarningsAsync(int staffId, decimal commission)
        {
            var earnings = await _calculateMoneyRepo.CalculateDailyEarningsAsync(staffId);
            return earnings * (commission / 100);
        }

        public async Task<decimal> GetWeeklyEarningsAsync(int staffId, decimal commission)
        {
            var earnings = await _calculateMoneyRepo.CalculateWeeklyEarningsAsync(staffId);
            return earnings * (commission / 100);
        }

        public async Task<decimal> GetMonthlyEarningsAsync(int staffId, decimal commission)
        {
            var earnings = await _calculateMoneyRepo.CalculateMonthlyEarningsAsync(staffId);
            return earnings * (commission / 100);
        }

        public async Task<decimal> GetDailyEarningsForSpecificDateAsync(int staffId, decimal commission, DateTime specificDate)
        {
            var earnings = await _calculateMoneyRepo.CalculateDailyEarningsForSpecificDateAsync(staffId, specificDate);
            return earnings * (commission / 100);
        }
    }
}