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
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public StaffRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
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

            var staffs = await _context.Staffs.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, staffs, TimeSpan.FromMinutes(10));

            return staffs;
        }

        public async Task<Staff> GetByIdAsync(int id)
        {
            string cacheKey = $"staff_{id}";
            var cachedStaff = await _redisHelper.GetCacheAsync<Staff>(cacheKey);
            if (cachedStaff != null)
            {
                return cachedStaff;
            }

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                throw new Exception($"Staff with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, staff, TimeSpan.FromMinutes(10));
            return staff;
        }

        public async Task<IEnumerable<Staff?>> GetAllByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"staff_business_{businessId}";
            var cachedStaffs = await _redisHelper.GetCacheAsync<List<Staff>>(cacheKey);

            if (cachedStaffs != null && cachedStaffs.Any())
            {
                // Ensure the cached staff list is sorted by StaffId before returning
                return cachedStaffs.OrderBy(s => s.StaffId);
            }

            var staffs = await _context.Staffs
                .Where(s => s.BusinessId == businessId)
                .OrderBy(s => s.StaffId)  // Sort the staff list by StaffId when fetching from the database
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, staffs, TimeSpan.FromMinutes(10));

            return staffs;
        }


        public async Task AddStaffAsync(Staff staff, int businessId)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null)
            {
                throw new Exception("Business not found");
            }

            staff.BusinessId = businessId;
            await _context.Staffs.AddAsync(staff);
            await _context.SaveChangesAsync();

            await UpdateStaffCacheAsync(staff);
            // Refresh related caches
            await RefreshRelatedCachesAsync(staff);
        }

        public async Task AddListStaffsAsync(IEnumerable<Staff>? staffs)
        {
            
            if (staffs == null)
            {
                throw new Exception("Staffs collection cannot be null or empty");
            }
            var staffList = staffs.ToList();
            var businessId = staffList.First().BusinessId;
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null)
            {
                throw new Exception("Business not found");
            }

            await _context.Staffs.AddRangeAsync(staffList);
            await _context.SaveChangesAsync();

            foreach (var staff in staffList)
            {
                if (staff != null)
                {
                    await RefreshRelatedCachesAsync(staff);
                }
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

            var staff = await _context.Staffs
                .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                .FirstOrDefaultAsync();

            if (staff == null)
            {
                throw new KeyNotFoundException($"Staff with ID {staffId} not found for Business ID {businessId}");
            }

            await _redisHelper.SetCacheAsync(cacheKey, staff, TimeSpan.FromMinutes(10));
            return staff;
        }

        public async Task UpdateAsync(Staff staff)
        {
            await using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // SQL query to update the staff in PostgreSQL
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

                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@Name", staff.Name);
                command.Parameters.AddWithValue("@Email", staff.Email);
                command.Parameters.AddWithValue("@Phone", staff.Phone);
                command.Parameters.AddWithValue("@Password", staff.Password);
                command.Parameters.AddWithValue("@BusinessId", staff.BusinessId);
                command.Parameters.AddWithValue("@StaffId", staff.StaffId);

                // Execute the update query
                await command.ExecuteNonQueryAsync();

                // Commit the transaction after successful update
                await transaction.CommitAsync();

                // After updating the database, refresh the cache
                await UpdateStaffCacheAsync(staff);
                await RefreshRelatedCachesAsync(staff);
            }
            catch
            {
                // Rollback transaction in case of any error
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task DeleteAsync(int businessId, int staffId)
        {
            var staff = await _context.Staffs
                .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                .FirstOrDefaultAsync();

            if (staff != null)
            {
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
                await InvalidateStaffCacheAsync(staff);
                // Refresh related caches
                await RefreshRelatedCachesAsync(staff);
            }
        }

        public async Task<Staff> GetByEmailAsync(string email)
        {
            var staff = await _context.Staffs.SingleOrDefaultAsync(s => s.Email == email);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Staff with email {email} not found");
            }
            return staff;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Staffs.AnyAsync(s => s.Email == email);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Staffs.AnyAsync(s => s.Phone == phone);
        }

        private async Task UpdateStaffCacheAsync(Staff staff)
        {
            var staffCacheKey = $"staff_{staff.StaffId}";
            await _redisHelper.SetCacheAsync(staffCacheKey, staff, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<Staff>(
                $"staff_business_{staff.BusinessId}",
                list =>
                {
                    // Find the index of the existing staff in the list
                    int index = list.FindIndex(s => s.StaffId == staff.StaffId);
                    if (index != -1)
                    {
                        // Replace the existing staff with the updated one
                        list[index] = staff;
                    }
                    else
                    {
                        // If the staff is not found in the list, add it to the list
                        list.Add(staff);
                    }

                    // Sort the list by StaffId
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
            // Refresh individual Staff cache
            var staffCacheKey = $"staff_{staff.StaffId}";
            await _redisHelper.SetCacheAsync(staffCacheKey, staff, TimeSpan.FromMinutes(10));

            // Refresh list of all Staff for the Business
            string businessCacheKey = $"staff_business_{staff.BusinessId}";
            var businessStaff = await _context.Staffs
                .Where(s => s.BusinessId == staff.BusinessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(businessCacheKey, businessStaff, TimeSpan.FromMinutes(10));

            // Optionally refresh the cache for all staff members if required
            const string allStaffCacheKey = "all_staffs";
            var allStaffs = await _context.Staffs.ToListAsync();
            await _redisHelper.SetCacheAsync(allStaffCacheKey, allStaffs, TimeSpan.FromMinutes(10));
        }

    }
}
