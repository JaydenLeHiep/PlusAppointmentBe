using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.CalculateMoneyRepo
{
    public class CalculateMoneyRepo : ICalculateMoneyRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public CalculateMoneyRepo(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<decimal> CalculateDailyEarningsAsync(int staffId)
        {
            return await CalculateEarningsAsync(staffId, TimeSpan.FromDays(1), "daily");
        }

        public async Task<decimal> CalculateWeeklyEarningsAsync(int staffId)
        {
            return await CalculateEarningsAsync(staffId, TimeSpan.FromDays(7), "weekly");
        }

        public async Task<decimal> CalculateMonthlyEarningsAsync(int staffId)
        {
            return await CalculateEarningsAsync(staffId, TimeSpan.FromDays(30), "monthly");
        }

        public async Task<decimal> CalculateDailyEarningsForSpecificDateAsync(int staffId, DateTime specificDate)
        {
            string cacheKey = $"earnings_staff_{staffId}_{specificDate:dd.MM}";

            var cachedEarnings = await _redisHelper.GetDecimalCacheAsync(cacheKey);

            if (cachedEarnings.HasValue)
            {
                return cachedEarnings.Value;
            }

            var utcSpecificDate = DateTime.SpecifyKind(specificDate, DateTimeKind.Utc);
            var appointments = await _context.Appointments
                .Where(a => a.StaffId == staffId && a.AppointmentTime.Date == utcSpecificDate && a.Status == "Done")
                .Include(a => a.AppointmentServices!)
                .ThenInclude(apptService => apptService.Service)
                .ToListAsync();

            if (!appointments.Any())
            {
                return 0;
            }

            
            decimal totalEarnings = appointments
                .SelectMany(a => a.AppointmentServices!)
                .Where(apptService => apptService.Service != null)
                .Sum(apptService => apptService.Service!.Price); // Ensure non-null access

            await _redisHelper.SetDecimalCacheAsync(cacheKey, totalEarnings, TimeSpan.FromMinutes(10));

            return totalEarnings;
        }

        private async Task<decimal> CalculateEarningsAsync(int staffId, TimeSpan timeSpan, string period)
        {
            var endDate = DateTime.UtcNow.Date;  // Use only the date part for consistency
            var startDate = endDate - timeSpan;

            string cacheKey = period switch
            {
                "daily" => $"earnings_staff_{staffId}_{endDate:dd.MM}",
                "weekly" => $"earnings_staff_{staffId}_{startDate:dd.MM}_{endDate:dd.MM}",
                "monthly" => $"earnings_staff_{staffId}_{startDate:dd.MM}_{endDate:dd.MM}",
                _ => throw new ArgumentException("Invalid period specified")
            };

            var cachedEarnings = await _redisHelper.GetDecimalCacheAsync(cacheKey);

            if (cachedEarnings.HasValue)
            {
                return cachedEarnings.Value;
            }


            var appointments = await _context.Appointments
                .Where(a => a.StaffId == staffId && a.AppointmentTime >= startDate && a.AppointmentTime <= endDate && a.Status == "Done")
                .Include(a => a.AppointmentServices!)
                    .ThenInclude(apptService => apptService.Service)
                .ToListAsync();

            if (!appointments.Any())
            {
                return 0;
            }

            decimal totalEarnings = appointments
                .SelectMany(a => a.AppointmentServices!)
                .Where(apptService => apptService.Service != null)
                .Sum(apptService => apptService.Service!.Price); // Ensure non-null access

            await _redisHelper.SetDecimalCacheAsync(cacheKey, totalEarnings, TimeSpan.FromMinutes(10));

            return totalEarnings;
        }

        public async Task InvalidateEarningsCacheAsync(int staffId)
        {
            var endDate = DateTime.UtcNow.Date;
            //var startDateDaily = endDate - TimeSpan.FromDays(1);
            var startDateWeekly = endDate - TimeSpan.FromDays(7);
            var startDateMonthly = endDate - TimeSpan.FromDays(30);

            var dailyCacheKey = $"earnings_staff_{staffId}_{endDate:dd.MM}";
            var weeklyCacheKey = $"earnings_staff_{staffId}_{startDateWeekly:dd.MM}_{endDate:dd.MM}";
            var monthlyCacheKey = $"earnings_staff_{staffId}_{startDateMonthly:dd.MM}_{endDate:dd.MM}";

            await _redisHelper.DeleteCacheAsync(dailyCacheKey);
            await _redisHelper.DeleteCacheAsync(weeklyCacheKey);
            await _redisHelper.DeleteCacheAsync(monthlyCacheKey);
        }
    }
}
