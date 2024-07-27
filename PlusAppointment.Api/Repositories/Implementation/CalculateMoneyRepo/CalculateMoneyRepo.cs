using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;

namespace PlusAppointment.Repositories.Implementation.CalculateMoneyRepo
{
    public class CalculateMoneyRepo : ICalculateMoneyRepo
    {
        private readonly ApplicationDbContext _context;

        public CalculateMoneyRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateDailyEarningsAsync(int staffId, decimal commission)
        {
            return await CalculateEarningsAsync(staffId, TimeSpan.FromDays(1), "daily", commission);
        }

        public async Task<decimal> CalculateWeeklyEarningsAsync(int staffId, decimal commission)
        {
            return await CalculateEarningsAsync(staffId, TimeSpan.FromDays(7), "weekly", commission);
        }

        public async Task<decimal> CalculateMonthlyEarningsAsync(int staffId, decimal commission)
        {
            return await CalculateEarningsAsync(staffId, TimeSpan.FromDays(30), "monthly", commission);
        }

        public async Task<decimal> CalculateDailyEarningsForSpecificDateAsync(int staffId, DateTime specificDate, decimal commission)
        {
            //string cacheKey = $"earnings_staff_{staffId}_{specificDate:dd.MM.yyyy}_{commission}";

            //var cachedEarnings = await _redisHelper.GetDecimalCacheAsync(cacheKey);

            //if (cachedEarnings.HasValue)
            //{
            //    return cachedEarnings.Value;
            //}

            var utcSpecificDate = DateTime.SpecifyKind(specificDate, DateTimeKind.Utc);
            var appointments = await _context.Appointments
                .Where(a => a.StaffId == staffId && a.AppointmentTime.Date == utcSpecificDate.Date && a.Status == "Done")
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

            var calculatedEarnings = totalEarnings * commission;
            //await _redisHelper.SetDecimalCacheAsync(cacheKey, calculatedEarnings, TimeSpan.FromMinutes(10));

            return calculatedEarnings;
        }

        private async Task<decimal> CalculateEarningsAsync(int staffId, TimeSpan timeSpan, string period, decimal commission)
        {
            var endDate = DateTime.UtcNow.Date;  // Use only the date part for consistency
            var startDate = endDate - timeSpan;

            //string cacheKey = period switch
            //{
            //    "daily" => $"earnings_staff_{staffId}_{endDate:dd.MM.yyyy}_{commission}",
            //    "weekly" => $"earnings_staff_{staffId}_{startDate:dd.MM.yyyy}_{endDate:dd.MM.yyyy}_{commission}",
            //    "monthly" => $"earnings_staff_{staffId}_{startDate:dd.MM.yyyy}_{endDate:dd.MM.yyyy}_{commission}",
            //    _ => throw new ArgumentException("Invalid period specified")
            //};

            //var cachedEarnings = await _redisHelper.GetDecimalCacheAsync(cacheKey);

            //if (cachedEarnings.HasValue)
            //{
            //    return cachedEarnings.Value;
            //}

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

            var calculatedEarnings = totalEarnings * commission;
            //await _redisHelper.SetDecimalCacheAsync(cacheKey, calculatedEarnings, TimeSpan.FromMinutes(10));

            return calculatedEarnings;
        }

        public async Task InvalidateEarningsCacheAsync(int staffId)
        {
            //var endDate = DateTime.UtcNow.Date;
            //var startDateWeekly = endDate - TimeSpan.FromDays(7);
            //var startDateMonthly = endDate - TimeSpan.FromDays(30);

            //var dailyCacheKey = $"earnings_staff_{staffId}_{endDate:dd.MM.yyyy}_*";
            //var weeklyCacheKey = $"earnings_staff_{staffId}_{startDateWeekly:dd.MM.yyyy}_{endDate:dd.MM.yyyy}_*";
            //var monthlyCacheKey = $"earnings_staff_{staffId}_{startDateMonthly:dd.MM.yyyy}_{endDate:dd.MM.yyyy}_*";

            //await _redisHelper.DeleteCacheAsync(dailyCacheKey);
            //await _redisHelper.DeleteCacheAsync(weeklyCacheKey);
            //await _redisHelper.DeleteCacheAsync(monthlyCacheKey);
        }
    }
}
