using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.StaffRepo
{
    public class StaffRepository : IStaffRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly RedisHelper _redisHelper;

        public StaffRepository(IDbContextFactory<ApplicationDbContext> contextFactory, RedisHelper redisHelper)
        {
            _contextFactory = contextFactory;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            const string cacheKey = "all_staffs";
            var cachedStaffs = await _redisHelper.GetCacheAsync<List<Staff>>(cacheKey);

            if (cachedStaffs != null && cachedStaffs.Any())
            {
                return cachedStaffs;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var staffs = await context.Staffs.ToListAsync();
                await _redisHelper.SetCacheAsync(cacheKey, staffs, TimeSpan.FromMinutes(10));
                return staffs;
            }
        }

        public async Task<Staff> GetByIdAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs.FindAsync(id);
                if (staff == null)
                {
                    throw new Exception($"Staff with ID {id} not found");
                }

                return staff;
            }
        }

        public async Task<IEnumerable<Staff?>> GetAllByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"staff_business_{businessId}";
            var cachedStaffs = await _redisHelper.GetCacheAsync<List<Staff>>(cacheKey);

            if (cachedStaffs != null && cachedStaffs.Any())
            {
                return cachedStaffs.OrderBy(s => s.StaffId);
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var staffs = await context.Staffs
                    .Where(s => s.BusinessId == businessId)
                    .OrderBy(s => s.StaffId)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(cacheKey, staffs, TimeSpan.FromMinutes(10));
                return staffs;
            }
        }

        public async Task AddStaffAsync(Staff staff, int businessId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var business = await context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    throw new Exception("Business not found");
                }

                staff.BusinessId = businessId;
                await context.Staffs.AddAsync(staff);
                await context.SaveChangesAsync();
            }

            await RefreshRelatedCachesAsync(staff);
        }

        public async Task AddListStaffsAsync(IEnumerable<Staff>? staffs)
        {
            if (staffs == null || !staffs.Any())
            {
                throw new Exception("Staffs collection cannot be null or empty");
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var staffList = staffs.ToList();
                var businessId = staffList.First().BusinessId;
                var business = await context.Businesses.FindAsync(businessId);
                if (business == null)
                {
                    throw new Exception("Business not found");
                }

                await context.Staffs.AddRangeAsync(staffList);
                await context.SaveChangesAsync();
            }

            foreach (var staff in staffs)
            {
                await UpdateStaffCacheAsync(staff);
            }
        }

        public async Task UpdateAsync(Staff staff)
        {
            await using var connection = new NpgsqlConnection(_contextFactory.CreateDbContext().Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var updateQuery = @"
                    UPDATE staffs 
                    SET 
                        name = @Name, 
                        email = @Email, 
                        phone = @Phone, 
                        password = @Password,
                        business_id = @BusinessId
                    WHERE staff_id = @StaffId";

                await using var command = new NpgsqlCommand(updateQuery, connection, transaction);

                command.Parameters.AddWithValue("@Name", staff.Name);
                command.Parameters.AddWithValue("@Email", staff.Email);
                command.Parameters.AddWithValue("@Phone", staff.Phone);
                command.Parameters.AddWithValue("@Password", staff.Password);
                command.Parameters.AddWithValue("@BusinessId", staff.BusinessId);
                command.Parameters.AddWithValue("@StaffId", staff.StaffId);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                await UpdateStaffCacheAsync(staff);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Staff?> GetByBusinessIdServiceIdAsync(int businessId, int staffId)
        {
            string cacheKey = $"staff_{staffId}";
            var cachedStaff = await _redisHelper.GetCacheAsync<Staff>(cacheKey);
            if (cachedStaff != null)
            {
                return cachedStaff;
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs
                    .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    throw new KeyNotFoundException($"Staff with ID {staffId} not found for Business ID {businessId}");
                }

                await _redisHelper.SetCacheAsync(cacheKey, staff, TimeSpan.FromMinutes(10));
                return staff;
            }
        }

        public async Task DeleteAsync(int businessId, int staffId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs
                    .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                    .FirstOrDefaultAsync();

                if (staff != null)
                {
                    context.Staffs.Remove(staff);
                    await context.SaveChangesAsync();
                    await InvalidateStaffCacheAsync(staff);
                    await RefreshRelatedCachesAsync(staff);
                }
            }
        }

        public async Task<Staff> GetByEmailAsync(string email)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staff = await context.Staffs.SingleOrDefaultAsync(s => s.Email == email);
                if (staff == null)
                {
                    throw new KeyNotFoundException($"Staff with email {email} not found");
                }

                return staff;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.Staffs.AnyAsync(s => s.Email == email);
            }
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.Staffs.AnyAsync(s => s.Phone == phone);
            }
        }

        private async Task UpdateStaffCacheAsync(Staff staff)
        {
            var staffCacheKey = $"staff_{staff.StaffId}";
            await _redisHelper.SetCacheAsync(staffCacheKey, staff, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<Staff>(
                $"staff_business_{staff.BusinessId}",
                list =>
                {
                    int index = list.FindIndex(s => s.StaffId == staff.StaffId);
                    if (index != -1)
                    {
                        list[index] = staff;
                    }
                    else
                    {
                        list.Add(staff);
                    }

                    return list.OrderBy(s => s.StaffId).ToList();
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateStaffCacheAsync(Staff staff)
        {
            var staffCacheKey = $"staff_{staff.StaffId}";
            await _redisHelper.DeleteCacheAsync(staffCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<Staff>(
                $"staff_business_{staff.BusinessId}",
                list =>
                {
                    list.RemoveAll(s => s.StaffId == staff.StaffId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(Staff staff)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var staffCacheKey = $"staff_{staff.StaffId}";
                await _redisHelper.SetCacheAsync(staffCacheKey, staff, TimeSpan.FromMinutes(10));

                string businessCacheKey = $"staff_business_{staff.BusinessId}";
                var businessStaff = await context.Staffs
                    .Where(s => s.BusinessId == staff.BusinessId)
                    .ToListAsync();

                await _redisHelper.SetCacheAsync(businessCacheKey, businessStaff, TimeSpan.FromMinutes(10));

                const string allStaffCacheKey = "all_staffs";
                var allStaffs = await context.Staffs.ToListAsync();
                await _redisHelper.SetCacheAsync(allStaffCacheKey, allStaffs, TimeSpan.FromMinutes(10));
            }
        }
    }
}
