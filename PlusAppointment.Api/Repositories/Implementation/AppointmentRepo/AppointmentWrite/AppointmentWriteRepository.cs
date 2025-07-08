using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Appointment;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentWrite
{
    public class AppointmentWriteRepository : IAppointmentWriteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicesRepository _servicesRepository;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppointmentWriteRepository(ApplicationDbContext context,
            IServicesRepository servicesRepository
        )
        {
            _context = context;
            _servicesRepository = servicesRepository;
        }
        
        public async Task AddAppointmentAsync(Appointment appointment)
        {
            try
            {
                if (appointment.AppointmentServices != null && appointment.AppointmentServices.Any())
                {
                    foreach (var mapping in appointment.AppointmentServices)
                    {
                        mapping.Appointment = appointment;
                        _context.AppointmentServiceStaffs.Add(mapping);
                    }

                    var serviceIds = appointment.AppointmentServices
                        .Select(x => x.ServiceId)
                        .Distinct()
                        .ToList();

                    var serviceDurations = await _context.Services
                        .Where(s => serviceIds.Contains(s.ServiceId))
                        .ToDictionaryAsync(s => s.ServiceId, s => s.Duration);

                    var totalDuration = appointment.AppointmentServices
                        .Select(mapping => serviceDurations[mapping.ServiceId])
                        .Aggregate(TimeSpan.Zero, (sum, next) => sum.Add(next));

                    appointment.Duration = totalDuration;
                }
                
                var trackedEntities = _context.ChangeTracker.Entries()
                    .Select(e => new
                    {
                        EntityType = e.Entity.GetType().Name,
                        State = e.State.ToString(),
                        Keys = string.Join(", ", e.Properties
                            .Where(p => p.Metadata.IsPrimaryKey())
                            .Select(p => $"{p.Metadata.Name}={p.CurrentValue}")
                        )
                    });

                foreach (var entry in trackedEntities)
                {
                    logger.Debug($"EF Tracking: {entry.EntityType} | State: {entry.State} | Keys: {entry.Keys}");
                }


                _context.Appointments.Add(appointment);

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.Error("Error inserting appointment: " + e);
                throw;
            }
        }
        
        public async Task UpdateAppointmentWithServicesAsync(int appointmentId, UpdateAppointmentDto updateDto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found");
            
            appointment.AppointmentTime = DateTime.SpecifyKind(updateDto.AppointmentTime,DateTimeKind.Utc);
            appointment.Comment = updateDto.Comment;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            if (appointment.AppointmentServices != null)
            {
                _context.AppointmentServiceStaffs.RemoveRange(appointment.AppointmentServices);
            }
            
            var newMappings = new List<AppointmentServiceStaffMapping>();
            TimeSpan totalDuration = TimeSpan.Zero;

            foreach (var serviceDto in updateDto.Services)
            {
                newMappings.Add(new AppointmentServiceStaffMapping
                {
                    AppointmentId = appointmentId,
                    ServiceId = serviceDto.ServiceId,
                    StaffId = serviceDto.StaffId
                });

                var service = await _servicesRepository.GetByIdAsync(serviceDto.ServiceId);
                var duration = serviceDto.UpdatedDuration ?? service.Duration;
                totalDuration += duration;
            }

            appointment.AppointmentServices = newMappings;
            appointment.Duration = totalDuration;

            await _context.SaveChangesAsync();
        }


        public async Task UpdateAppointmentStatusAsync(Appointment appointment)
        {
            _context.Entry(appointment).Property(a => a.Status).IsModified = true;
            _context.Entry(appointment).Property(a => a.UpdatedAt).IsModified = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found.");
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAppointmentForCustomerAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found.");
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAppointmentsBefore(DateTime date)
        {
            var appointments = await _context.Appointments
                .Where(a => a.AppointmentTime < date)
                .ToListAsync();
            
            if (!appointments.Any())
            {
                logger.Warn("No appointments to delete");
                return false;
            }

            _context.Appointments.RemoveRange(appointments);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}