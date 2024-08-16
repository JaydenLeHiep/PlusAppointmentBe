using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo;
using PlusAppointment.Utils.Redis;
using System.Linq;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo
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

        private DateTime GetStartOfTodayUtc()
        {
            return DateTime.UtcNow.Date;
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
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

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
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");
            }

            appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);

            appointmentCacheDto = MapToCacheDto(appointment);
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDto, TimeSpan.FromMinutes(10));
            return appointment;
        }

        public async Task<bool> IsStaffAvailable(int staffId, DateTime appointmentTime, TimeSpan duration)
        {
            var endTime = appointmentTime.Add(duration);

            var appointments = await _context.Appointments
                .Include(a => a.AppointmentServices)
                .Where(a => a.AppointmentServices != null &&
                            a.AppointmentServices.Any(apptService => apptService.StaffId == staffId))
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var existingAppointmentEndTime = appointment.AppointmentTime.Add(appointment.Duration);
                if (appointment.AppointmentTime < endTime && existingAppointmentEndTime > appointmentTime)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<Customer?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            // Load the services from the database based on the service IDs in the mappings
            if (appointment.AppointmentServices != null)
            {
                var serviceIds = appointment.AppointmentServices.Select(mapping => mapping.ServiceId).ToList();
                var services = await _context.Services
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .ToDictionaryAsync(s => s.ServiceId, s => s);

                // Calculate the total duration based on the loaded services
                var totalDuration = appointment.AppointmentServices
                    .Select(mapping => services[mapping.ServiceId].Duration)
                    .Aggregate(TimeSpan.Zero, (sum, next) => sum.Add(next));

                // Update the appointment's duration
                appointment.Duration = totalDuration;
            }

            // Save the appointment to the database
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            await UpdateAppointmentCacheAsync(appointment);
        }


        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            await UpdateAppointmentCacheAsync(appointment);
        }

        public async Task UpdateAppointmentWithServicesAsync(int appointmentId,
            UpdateAppointmentDto updateAppointmentDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the appointment with all related services and staff mappings
                    var appointment = await _context.Appointments
                        .Include(a => a.AppointmentServices)
                        .ThenInclude(asm => asm.Service)
                        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                    if (appointment == null)
                    {
                        throw new KeyNotFoundException("Appointment not found");
                    }

                    // Update appointment basic details
                    appointment.AppointmentTime = updateAppointmentDto.AppointmentTime;
                    appointment.Comment = updateAppointmentDto.Comment;
                    appointment.UpdatedAt = DateTime.UtcNow;

                    // Remove old mappings not present in the update DTO
                    var mappingsToRemove = appointment.AppointmentServices
                        .Where(cm => !updateAppointmentDto.Services.Any(ns =>
                            ns.ServiceId == cm.ServiceId && ns.StaffId == cm.StaffId))
                        .ToList();

                    foreach (var mappingToRemove in mappingsToRemove)
                    {
                        _context.AppointmentServiceStaffs.Remove(mappingToRemove);
                    }

                    // Add new mappings
                    foreach (var serviceDto in updateAppointmentDto.Services)
                    {
                        if (!appointment.AppointmentServices.Any(cm =>
                                cm.ServiceId == serviceDto.ServiceId && cm.StaffId == serviceDto.StaffId))
                        {
                            appointment.AppointmentServices.Add(new AppointmentServiceStaffMapping
                            {
                                AppointmentId = appointmentId,
                                ServiceId = serviceDto.ServiceId,
                                StaffId = serviceDto.StaffId
                            });
                        }
                    }

                    // Recalculate the total duration
                    TimeSpan totalDuration = TimeSpan.Zero;

                    foreach (var serviceDto in updateAppointmentDto.Services)
                    {
                        var service = await _context.Services.FindAsync(serviceDto.ServiceId);
                        if (service != null)
                        {
                            totalDuration += serviceDto.UpdatedDuration ?? service.Duration;
                        }
                    }

                    // Update the appointment's total duration
                    appointment.Duration = totalDuration;

                    // Save changes and commit the transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Update cache after successful transaction
                    await UpdateAppointmentCacheAsync(appointment);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }


        public async Task UpdateAppointmentStatusAsync(Appointment appointment)
        {
            _context.Attach(appointment);
            _context.Entry(appointment).Property(a => a.Status).IsModified = true;
            _context.Entry(appointment).Property(a => a.UpdatedAt).IsModified = true;

            await _context.SaveChangesAsync();

            await UpdateAppointmentCacheAsync(appointment);
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

            await InvalidateAppointmentCacheAsync(appointment);
        }


        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            var startOfTodayUtc = GetStartOfTodayUtc();
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.CustomerId == customerId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            var startOfTodayUtc = GetStartOfTodayUtc();
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.BusinessId == businessId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            var startOfTodayUtc = GetStartOfTodayUtc();

            var appointmentsQuery = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.AppointmentServices != null &&
                            a.AppointmentServices.Any(apptService => apptService.StaffId == staffId) &&
                            a.AppointmentTime >= startOfTodayUtc);

            var appointments = await appointmentsQuery.ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            return appointments;
        }

        private async Task UpdateAppointmentCacheAsync(Appointment appointment)
        {
            var appointmentCacheKey = $"appointment_{appointment.AppointmentId}";
            var appointmentCacheDto = MapToCacheDto(appointment);

            // Set cache with 24-hour expiration
            await _redisHelper.SetCacheAsync(appointmentCacheKey, appointmentCacheDto, TimeSpan.FromDays(1));

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_customer_{appointment.CustomerId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromDays(1)); // Set list cache with 24-hour expiration

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_business_{appointment.BusinessId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromDays(1)); // Set list cache with 24-hour expiration

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_staff_{appointment.AppointmentServices?.FirstOrDefault()?.StaffId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromDays(1)); // Set list cache with 24-hour expiration
        }

        private async Task InvalidateAppointmentCacheAsync(Appointment appointment)
        {
            var appointmentCacheKey = $"appointment_{appointment.AppointmentId}";
            await _redisHelper.DeleteCacheAsync(appointmentCacheKey);

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_customer_{appointment.CustomerId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromDays(1)); // Set list cache with 24-hour expiration

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_business_{appointment.BusinessId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromDays(1)); // Set list cache with 24-hour expiration

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_staff_{appointment.AppointmentServices?.FirstOrDefault()?.StaffId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromDays(1)); // Set list cache with 24-hour expiration
        }

        private AppointmentCacheDto MapToCacheDto(Appointment appointment)
        {
            _context.Entry(appointment).Reference(a => a.Customer).Load();

            if (appointment.AppointmentServices != null)
            {
                _context.Entry(appointment).Collection(a => a.AppointmentServices!).Query()
                    .Include(apptService => apptService.Service)
                    .Include(apptService => apptService.Staff)
                    .Load();
            }

            var serviceStaffs = appointment.AppointmentServices?
                .Select(apptService => new ServiceStaffCacheDto
                {
                    ServiceId = apptService.ServiceId,
                    ServiceName = apptService.Service?.Name ?? string.Empty,
                    StaffId = apptService.StaffId,
                    StaffName = apptService.Staff?.Name ?? string.Empty,
                    StaffPhone = apptService.Staff?.Phone ?? string.Empty
                }).ToList() ?? new List<ServiceStaffCacheDto>();

            return new AppointmentCacheDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? string.Empty,
                CustomerPhone = appointment.Customer?.Phone ?? string.Empty,
                BusinessId = appointment.BusinessId,
                AppointmentTime = appointment.AppointmentTime,
                Duration = appointment.Duration,
                Comment = appointment.Comment ?? string.Empty,
                Status = appointment.Status,
                ServiceStaffs = serviceStaffs
            };
        }

        private Appointment MapFromCacheDto(AppointmentCacheDto dto)
        {
            var services = dto.ServiceStaffs.Select(ss => new AppointmentServiceStaffMapping
            {
                ServiceId = ss.ServiceId,
                StaffId = ss.StaffId,
                Service = _context.Services.FirstOrDefault(s => s.ServiceId == ss.ServiceId),
                Staff = _context.Staffs.FirstOrDefault(s => s.StaffId == ss.StaffId)
            }).ToList();

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
                AppointmentTime = dto.AppointmentTime,
                Duration = dto.Duration,
                Comment = dto.Comment,
                Status = dto.Status,
                AppointmentServices = services
            };
        }

        private DateTime ConvertToLocalTime(DateTime utcTime)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);
        }
    }
}