using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.CheckInRepo;

public class CheckInRepository : ICheckInRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly RedisHelper _redisHelper;

    public CheckInRepository(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
    {
        _contextFactory = contextFactory;
        _redisHelper = redisHelper;
    }

    public async Task<IEnumerable<CheckIn?>> GetAllCheckInsAsync()
    {
        const string cacheKey = "all_checkins";
        var cachedCheckIns = await _redisHelper.GetCacheAsync<List<CheckIn?>>(cacheKey);

        if (cachedCheckIns != null && cachedCheckIns.Any())
        {
            return cachedCheckIns;
        }

        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIns = await context.CheckIns.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, checkIns, TimeSpan.FromMinutes(10));
            return checkIns;
        }
    }

    public async Task<CheckIn?> GetCheckInByIdAsync(int checkInId)
    {
        string cacheKey = $"checkin_{checkInId}";
        var cachedCheckIn = await _redisHelper.GetCacheAsync<CheckIn>(cacheKey);
        if (cachedCheckIn != null)
        {
            return cachedCheckIn;
        }

        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIn = await context.CheckIns.FindAsync(checkInId);
            if (checkIn == null)
            {
                throw new KeyNotFoundException($"CheckIn with ID {checkInId} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, checkIn, TimeSpan.FromMinutes(10));
            return checkIn;
        }
    }

    public async Task<IEnumerable<CheckIn?>> GetCheckInsByBusinessIdAsync(int businessId)
    {
        string cacheKey = $"checkins_business_{businessId}";
        var cachedCheckIns = await _redisHelper.GetCacheAsync<List<CheckIn?>>(cacheKey);

        if (cachedCheckIns != null && cachedCheckIns.Any())
        {
            return cachedCheckIns.OrderBy(c => c.CheckInTime);
        }

        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIns = await context.CheckIns
                .Where(c => c.BusinessId == businessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, checkIns, TimeSpan.FromMinutes(10));
            return checkIns;
        }
    }

    public async Task AddCheckInAsync(CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn));
        }

        using (var context = _contextFactory.CreateDbContext())
        {
            context.CheckIns.Add(checkIn);
            await context.SaveChangesAsync();
        }

        await RefreshRelatedCachesAsync(checkIn);
    }

    public async Task UpdateCheckInAsync(CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn));
        }

        await using var connection =
            new NpgsqlConnection(_contextFactory.CreateDbContext().Database.GetConnectionString());
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var updateQuery = @"
                    UPDATE check_ins
                    SET 
                        customer_id = @CustomerId,
                        business_id = @BusinessId,
                        check_in_time = @CheckInTime,
                        check_in_type = @CheckInType
                    WHERE check_in_id = @CheckInId";

            await using var command = new NpgsqlCommand(updateQuery, connection, transaction);

            command.Parameters.AddWithValue("@CustomerId", checkIn.CustomerId);
            command.Parameters.AddWithValue("@BusinessId", checkIn.BusinessId);
            command.Parameters.AddWithValue("@CheckInTime", checkIn.CheckInTime);
            command.Parameters.AddWithValue("@CheckInType", checkIn.CheckInType.ToString());
            command.Parameters.AddWithValue("@CheckInId", checkIn.CheckInId);

            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            await RefreshRelatedCachesAsync(checkIn);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteCheckInAsync(int checkInId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIn = await context.CheckIns.FindAsync(checkInId);
            if (checkIn != null)
            {
                context.CheckIns.Remove(checkIn);
                await context.SaveChangesAsync();

                await InvalidateCheckInCacheAsync(checkIn);
                await RefreshRelatedCachesAsync(checkIn);
            }
        }
    }
    
    public async Task<bool> HasCheckedInTodayAsync(int businessId, int customerId, DateTime checkInDate)
    {
        using var context = _contextFactory.CreateDbContext();
        var startOfDay = checkInDate.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await context.CheckIns.AnyAsync(ci =>
            ci.BusinessId == businessId &&
            ci.CustomerId == customerId &&
            ci.CheckInTime >= startOfDay &&
            ci.CheckInTime <= endOfDay);
    }

    // Private helper methods to handle caching
    private async Task InvalidateCheckInCacheAsync(CheckIn checkIn)
    {
        var checkInCacheKey = $"checkin_{checkIn.CheckInId}";
        await _redisHelper.DeleteCacheAsync(checkInCacheKey);

        await _redisHelper.RemoveFromListCacheAsync<CheckIn>(
            $"checkins_business_{checkIn.BusinessId}",
            list =>
            {
                list.RemoveAll(c => c.CheckInId == checkIn.CheckInId);
                return list;
            },
            TimeSpan.FromMinutes(10));
    }

    private async Task RefreshRelatedCachesAsync(CheckIn? checkIn)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var checkInCacheKey = $"checkin_{checkIn.CheckInId}";
            await _redisHelper.SetCacheAsync(checkInCacheKey, checkIn, TimeSpan.FromMinutes(10));

            string businessCacheKey = $"checkins_business_{checkIn.BusinessId}";
            var businessCheckIns = await context.CheckIns
                .Where(c => c.BusinessId == checkIn.BusinessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(businessCacheKey, businessCheckIns, TimeSpan.FromMinutes(10));

            const string allCheckInsCacheKey = "all_checkins";
            var allCheckIns = await context.CheckIns.ToListAsync();
            await _redisHelper.SetCacheAsync(allCheckInsCacheKey, allCheckIns, TimeSpan.FromMinutes(10));
        }
    }
}