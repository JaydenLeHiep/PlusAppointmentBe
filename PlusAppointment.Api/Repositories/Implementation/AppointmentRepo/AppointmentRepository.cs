using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;
using WebApplication1.Data;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;
using WebApplication1.Utils.Redis;


namespace WebApplication1.Repositories.Implementation.AppointmentRepo
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public AppointmentRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            const string cacheKey = "all_appointments";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<Appointment>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments;
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, appointments, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            string cacheKey = $"appointment_{appointmentId}";
            var appointment = await _redisHelper.GetCacheAsync<Appointment>(cacheKey);
            if (appointment != null)
            {
                return appointment;
            }

            appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, appointment, TimeSpan.FromMinutes(10));
            return appointment;
        }
        public async Task<bool> IsStaffAvailable(int staffId, DateTime appointmentTime, TimeSpan duration)
        {
            // Calculate the end time of the new appointment in C#
            var endTime = appointmentTime.Add(duration);

            // Retrieve all appointments for the staff
            var appointments = await _context.Appointments
                .Where(a => a.StaffId == staffId)
                .ToListAsync();

            // Check for overlapping appointments
            foreach (var appointment in appointments)
            {
                var existingAppointmentEndTime = appointment.AppointmentTime.Add(appointment.Duration);
                if (appointment.AppointmentTime < endTime && existingAppointmentEndTime > appointmentTime)
                {
                    return false; // Overlap found
                }
            }

            return true; // No overlap
        }

        
        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            await InvalidateCache();
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            await InvalidateCache();
        }

        public async Task DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                await InvalidateCache();
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            string cacheKey = $"appointments_customer_{customerId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<Appointment>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments;
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, appointments, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"appointments_business_{businessId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<Appointment>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments;
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.BusinessId == businessId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, appointments, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            string cacheKey = $"appointments_staff_{staffId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<Appointment>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments;
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.StaffId == staffId)
                .ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, appointments, TimeSpan.FromMinutes(10));

            return appointments;
        }

        private async Task InvalidateCache()
        {
            await _redisHelper.DeleteKeysByPatternAsync("appointment_*");
            await _redisHelper.DeleteCacheAsync("all_appointments");
        }
    }
}