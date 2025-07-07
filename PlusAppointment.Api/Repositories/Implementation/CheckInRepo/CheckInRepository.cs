using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;

namespace PlusAppointment.Repositories.Implementation.CheckInRepo;

public class CheckInRepository : ICheckInRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CheckInRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<CheckIn?>> GetAllCheckInsAsync()
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIns = await context.CheckIns.ToListAsync();
            return checkIns;
        }
    }

    public async Task<CheckIn?> GetCheckInByIdAsync(int checkInId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIn = await context.CheckIns.FindAsync(checkInId);
            if (checkIn == null)
            {
                throw new KeyNotFoundException($"CheckIn with ID {checkInId} not found");
            }
            
            return checkIn;
        }
    }

    public async Task<IEnumerable<CheckIn?>> GetCheckInsByBusinessIdAsync(int businessId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var checkIns = await context.CheckIns
                .Where(c => c.BusinessId == businessId)
                .ToListAsync();
            
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
}