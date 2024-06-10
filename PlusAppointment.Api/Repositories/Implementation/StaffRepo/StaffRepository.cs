using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using WebApplication1.Data;
using WebApplication1.Repositories.Interfaces.StaffRepo;
using WebApplication1.Utils.Redis;

namespace WebApplication1.Repositories.Implementation.StaffRepo
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
            var staffs = await _redisHelper.GetCacheAsync<IEnumerable<Staff>>(cacheKey);
            if (staffs != null && staffs.Any())
            {
                return staffs;
            }

            staffs = await _context.Staffs.ToListAsync();
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

            await InvalidateCache();
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

            await InvalidateCache();
        }

        public async Task UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();

            await InvalidateCache();
        }

        public async Task DeleteAsync(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff != null)
            {
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
            }

            await InvalidateCache();
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

        private async Task InvalidateCache()
        {
            await _redisHelper.DeleteKeysByPatternAsync("staff_*");
            await _redisHelper.DeleteCacheAsync("all_staffs");
        }
    }
}