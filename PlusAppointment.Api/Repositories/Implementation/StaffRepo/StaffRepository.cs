using Microsoft.EntityFrameworkCore;
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
            var staff = await _redisHelper.GetCacheAsync<Staff>(cacheKey);
            if (staff != null)
            {
                return staff;
            }

            staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Staff with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, staff, TimeSpan.FromMinutes(10));
            return staff;
        }

        public async Task<IEnumerable<Staff?>> GetAllByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"staff_business_{businessId}";
            var cachedStaff = await _redisHelper.GetCacheAsync<List<Staff>>(cacheKey);

            if (cachedStaff != null && cachedStaff.Any())
            {
                return cachedStaff;
            }

            var staff = await _context.Staffs.Where(s => s.BusinessId == businessId).ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, staff, TimeSpan.FromMinutes(10));

            return staff;
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
        }

        public async Task AddListStaffsAsync(IEnumerable<Staff> staffs)
        {
            var staffList = staffs.ToList();
            if (staffs == null || !staffList.Any())
            {
                throw new ArgumentException("Staffs collection cannot be null or empty", nameof(staffs));
            }

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
                    await UpdateStaffCacheAsync(staff);
                }
            }
        }

        public async Task<Staff?> GetByBusinessIdServiceIdAsync(int businessId, int staffId)
        {
            return await _context.Staffs
                .Where(s => s.BusinessId == businessId && s.StaffId == staffId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();

            await UpdateStaffCacheAsync(staff);
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
                    list.RemoveAll(s => s.StaffId == staff.StaffId);
                    list.Add(staff);
                    return list;
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
    }
}
