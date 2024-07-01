using Microsoft.EntityFrameworkCore;
using PlusAppointment.Models.Classes;

using PlusAppointment.Models.DTOs;
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
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments.Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .ToListAsync();

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            string cacheKey = $"appointment_{appointmentId}";
            var appointmentCacheDto = await _redisHelper.GetCacheAsync<AppointmentCacheDto>(cacheKey);
            if (appointmentCacheDto != null)
            {
                return MapFromCacheDto(appointmentCacheDto);
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");
            }

            appointmentCacheDto = MapToCacheDto(appointment);
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDto, TimeSpan.FromMinutes(10));
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

            await InvalidateCache(appointment);
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            await InvalidateCache(appointment);
        }

        public async Task DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                await InvalidateCache(appointment);
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            string cacheKey = $"appointments_customer_{customerId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments
                    .Where(dto => dto.AppointmentTime >= DateTime.UtcNow)
                    .Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.CustomerId == customerId && a.AppointmentTime >= DateTime.UtcNow)
                .ToListAsync();

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }


        public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"appointments_business_{businessId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments
                    .Where(dto => dto.AppointmentTime >= DateTime.UtcNow)
                    .Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.BusinessId == businessId && a.AppointmentTime >= DateTime.UtcNow)
                .ToListAsync();

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }


        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            string cacheKey = $"appointments_staff_{staffId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments
                    .Where(dto => dto.AppointmentTime >= DateTime.UtcNow)
                    .Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.StaffId == staffId && a.AppointmentTime >= DateTime.UtcNow)
                .ToListAsync();

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }


        private async Task InvalidateCache(Appointment appointment)
        {
            await _redisHelper.DeleteKeysByPatternAsync("appointment_*");
            await _redisHelper.DeleteCacheAsync("all_appointments");

            if (appointment != null)
            {
                await _redisHelper.DeleteCacheAsync($"appointments_customer_{appointment.CustomerId}");
                await _redisHelper.DeleteCacheAsync($"appointments_business_{appointment.BusinessId}");
                await _redisHelper.DeleteCacheAsync($"appointments_staff_{appointment.StaffId}");
            }
        }

        private AppointmentCacheDto MapToCacheDto(Appointment appointment)
        {
            return new AppointmentCacheDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? string.Empty,
                CustomerPhone = appointment.Customer?.Phone ?? string.Empty,
                BusinessId = appointment.BusinessId,
                ServiceId = appointment.ServiceId,
                ServiceName = appointment.Service?.Name ?? string.Empty,
                ServiceDuration = appointment.Service?.Duration ?? TimeSpan.Zero,
                ServicePrice = appointment.Service?.Price ?? 0,
                StaffId = appointment.StaffId,
                StaffName = appointment.Staff?.Name ?? string.Empty,
                StaffPhone = appointment.Staff?.Phone ?? string.Empty,
                AppointmentTime = appointment.AppointmentTime,
                Duration = appointment.Duration,
                Status = appointment.Status
            };
        }

        private Appointment MapFromCacheDto(AppointmentCacheDto dto)
        {
            return new Appointment
            {
                AppointmentId = dto.AppointmentId,
                CustomerId = dto.CustomerId,
                Customer = new Customer
                {
                    CustomerId = dto.CustomerId,
                    Name = dto.CustomerName,
                    Phone = dto.CustomerPhone
                },
                BusinessId = dto.BusinessId,
                ServiceId = dto.ServiceId,
                Service = new Service
                {
                    ServiceId = dto.ServiceId,
                    Name = dto.ServiceName,
                    Duration = dto.ServiceDuration,
                    Price = dto.ServicePrice
                },
                StaffId = dto.StaffId,
                Staff = new Staff
                {
                    StaffId = dto.StaffId,
                    Name = dto.StaffName,
                    Phone = dto.StaffPhone
                },
                AppointmentTime = dto.AppointmentTime,
                Duration = dto.Duration,
                Status = dto.Status
            };
        }
    }
}
