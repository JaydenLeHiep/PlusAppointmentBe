using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.NotAvailableDateRepository;

namespace PlusAppointment.Repositories.Implementation.NotAvailableDateRepository
{
    public class NotAvailableDateRepository : INotAvailableDateRepository
    {
        private readonly ApplicationDbContext _context;

        public NotAvailableDateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotAvailableDate>> GetAllByBusinessIdAsync(int businessId)
        {
            var notAvailableDates = await _context.NotAvailableDates
                .Where(nad => nad.BusinessId == businessId)
                .ToListAsync();

            return notAvailableDates;
        }

        public async Task<IEnumerable<NotAvailableDate>> GetAllByStaffIdAsync(int businessId, int staffId)
        {
            var notAvailableDates = await _context.NotAvailableDates
                .Where(nad => nad.BusinessId == businessId && nad.StaffId == staffId)
                .ToListAsync();

            return notAvailableDates;
        }

        public async Task<NotAvailableDate?> GetByIdAsync(int businessId, int staffId, int id)
        {

            var notAvailableDate = await _context.NotAvailableDates
                .FirstOrDefaultAsync(nad =>
                    nad.BusinessId == businessId && nad.StaffId == staffId && nad.NotAvailableDateId == id);

            return notAvailableDate;
        }

        public async Task AddAsync(NotAvailableDate notAvailableDate)
        {
            await _context.NotAvailableDates.AddAsync(notAvailableDate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NotAvailableDate notAvailableDate)
        {
            _context.NotAvailableDates.Update(notAvailableDate);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int businessId, int staffId, int id)
        {
            var notAvailableDate = await GetByIdAsync(businessId, staffId, id);
            if (notAvailableDate != null)
            {
                _context.NotAvailableDates.Remove(notAvailableDate);
                await _context.SaveChangesAsync();
            }
        }
    }
}