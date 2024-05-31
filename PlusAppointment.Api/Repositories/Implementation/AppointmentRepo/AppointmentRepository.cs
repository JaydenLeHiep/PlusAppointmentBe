using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;

namespace WebApplication1.Repositories.Implementation.AppointmentRepo;

public class AppointmentRepository: IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                                 .Include(a => a.Customer)
                                 .Include(a => a.Business)
                                 .Include(a => a.Service)
                                 .Include(a => a.Staff)
                                 .ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                                 .Include(a => a.Customer)
                                 .Include(a => a.Business)
                                 .Include(a => a.Service)
                                 .Include(a => a.Staff)
                                 .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Entry(appointment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            return await _context.Appointments
                                 .Include(a => a.Customer)
                                 .Include(a => a.Business)
                                 .Include(a => a.Service)
                                 .Include(a => a.Staff)
                                 .Where(a => a.CustomerId == customerId)
                                 .ToListAsync();
        }
        
        public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.StaffId == staffId)
                .ToListAsync();
        }
}