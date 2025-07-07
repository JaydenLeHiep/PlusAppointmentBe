using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotAvailableTimeRepo;

namespace PlusAppointment.Repositories.Implementation.NotAvailableTimeRepo
{
    public class NotAvailableTimeRepository : INotAvailableTimeRepository
    {
        private readonly ApplicationDbContext _context;

        public NotAvailableTimeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotAvailableTime>> GetAllByBusinessIdAsync(int businessId)
        {
            var notAvailableTimes = await _context.NotAvailableTimes
                .Where(nat => nat.BusinessId == businessId)
                .ToListAsync();

            return notAvailableTimes;
        }

        public async Task<IEnumerable<NotAvailableTime>> GetAllByStaffIdAsync(int businessId, int staffId)
        {
            var notAvailableTimes = await _context.NotAvailableTimes
                .Where(nat => nat.BusinessId == businessId && nat.StaffId == staffId)
                .ToListAsync();

            return notAvailableTimes;
        }

        public async Task<NotAvailableTime?> GetByIdAsync(int businessId, int staffId, int id)
        {
            var notAvailableTime = await _context.NotAvailableTimes
                .FirstOrDefaultAsync(nat =>
                    nat.BusinessId == businessId && nat.StaffId == staffId && nat.NotAvailableTimeId == id);

            return notAvailableTime;
        }

        public async Task AddAsync(NotAvailableTime notAvailableTime)
        {
            await _context.NotAvailableTimes.AddAsync(notAvailableTime);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NotAvailableTime notAvailableTime)
        {
            _context.NotAvailableTimes.Update(notAvailableTime);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int businessId, int staffId, int id)
        {
            var notAvailableTime = await GetByIdAsync(businessId, staffId, id);
            if (notAvailableTime != null)
            {
                _context.NotAvailableTimes.Remove(notAvailableTime);
                await _context.SaveChangesAsync();
            }
        }
    }
}