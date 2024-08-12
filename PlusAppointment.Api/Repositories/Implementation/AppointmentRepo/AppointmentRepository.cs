using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo;
using PlusAppointment.Repositories.Interfaces.CalculateMoneyRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;
        //private readonly ICalculateMoneyRepo _calculateMoneyRepo;

        public AppointmentRepository(ApplicationDbContext context, RedisHelper redisHelper,
            ICalculateMoneyRepo calculateMoneyRepo)
        {
            _context = context;
            _redisHelper = redisHelper;
            //_calculateMoneyRepo = calculateMoneyRepo;
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
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service!)
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
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
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
                .Where(a => a.StaffId == staffId)
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
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            await UpdateAppointmentCacheAsync(appointment);
            //await _calculateMoneyRepo.InvalidateEarningsCacheAsync(appointment.StaffId);
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            await UpdateAppointmentCacheAsync(appointment);
            //await _calculateMoneyRepo.InvalidateEarningsCacheAsync(appointment.StaffId);
        }

        // it can not add to duplication of a service in a appointment
        public async Task UpdateAppointmentWithServicesAsync(int appointmentId,
            UpdateAppointmentDto updateAppointmentDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the current appointment
                    var appointment = await _context.Appointments
                        .Include(a => a.AppointmentServices)!
                        .ThenInclude(asm => asm.Service)
                        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                    if (appointment == null)
                    {
                        throw new KeyNotFoundException("Appointment not found");
                    }

                    // Update appointment time and comment
                    appointment.AppointmentTime = updateAppointmentDto.AppointmentTime;
                    appointment.Comment = updateAppointmentDto.Comment;
                    appointment.UpdatedAt = DateTime.UtcNow;

                    // Get the current services associated with the appointment
                    if (appointment.AppointmentServices != null)
                    {
                        var currentServiceIds = appointment.AppointmentServices.Select(asm => asm.ServiceId).ToList();
                        var updatedServiceIds = updateAppointmentDto.Services.Select(s => s.ServiceId).ToList();

                        // Identify services to remove
                        var servicesToRemove = appointment.AppointmentServices
                            .Where(asm => !updatedServiceIds.Contains(asm.ServiceId))
                            .ToList();

                        foreach (var serviceToRemove in servicesToRemove)
                        {
                            appointment.AppointmentServices.Remove(serviceToRemove);
                        }

                        // Identify services to add and add them to the appointment
                        var servicesToAdd = updateAppointmentDto.Services
                            .Where(s => !currentServiceIds.Contains(s.ServiceId))
                            .ToList();

                        foreach (var serviceDto in servicesToAdd)
                        {
                            // Fetch the service details from the database
                            var service = await _context.Services.FindAsync(serviceDto.ServiceId);
                            if (service != null)
                            {
                                appointment.AppointmentServices.Add(new AppointmentServiceMapping
                                {
                                    AppointmentId = appointmentId,
                                    ServiceId = serviceDto.ServiceId,
                                    Service = service
                                });
                            }
                        }
                    }

                    // Recalculate the total duration
                    TimeSpan totalDuration = TimeSpan.Zero;

                    foreach (var serviceDto in updateAppointmentDto.Services)
                    {
                        // Check if service is new or existing
                        var appointmentService = appointment.AppointmentServices
                            .FirstOrDefault(asm => asm.ServiceId == serviceDto.ServiceId);

                        if (serviceDto.UpdatedDuration.HasValue)
                        {
                            // Use updated duration provided by user
                            totalDuration += serviceDto.UpdatedDuration.Value;
                        }
                        else if (appointmentService != null && appointmentService.Service != null)
                        {
                            // Use original service duration
                            totalDuration += appointmentService.Service.Duration;
                        }
                    }

                    // Update the appointment's total duration
                    appointment.Duration = totalDuration;

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    // Update the cache if necessary
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

            if (appointment.AppointmentServices != null)
                foreach (var service in appointment.AppointmentServices)
                {
                    _context.Entry(service).State = EntityState.Unchanged;
                }

            await _context.SaveChangesAsync();

            await UpdateAppointmentCacheAsync(appointment);
            //await _calculateMoneyRepo.InvalidateEarningsCacheAsync(appointment.StaffId);
        }

        public async Task DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                await InvalidateAppointmentCacheAsync(appointment);
                //await _calculateMoneyRepo.InvalidateEarningsCacheAsync(appointment.StaffId);
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            string cacheKey = $"appointments_customer_{customerId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);
            var startOfTodayUtc = GetStartOfTodayUtc();

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments
                    .Where(dto => dto.AppointmentTime >= startOfTodayUtc)
                    .Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Where(a => a.CustomerId == customerId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId)
        {
            string cacheKey = $"appointments_customer_history_{customerId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments.Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }


        public async Task<IEnumerable<Appointment>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"appointments_business_{businessId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);
            var startOfTodayUtc = GetStartOfTodayUtc();

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments
                    .Where(dto => dto.AppointmentTime >= startOfTodayUtc)
                    .Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Where(a => a.BusinessId == businessId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            string cacheKey = $"appointments_staff_{staffId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);
            var startOfTodayUtc = GetStartOfTodayUtc();

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments
                    .Where(dto => dto.AppointmentTime >= startOfTodayUtc)
                    .Select(dto => MapFromCacheDto(dto));
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Where(a => a.StaffId == staffId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.AppointmentTime = ConvertToLocalTime(appointment.AppointmentTime);
            }

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }

        private async Task UpdateAppointmentCacheAsync(Appointment appointment)
        {
            var appointmentCacheKey = $"appointment_{appointment.AppointmentId}";
            var appointmentCacheDto = MapToCacheDto(appointment);
            await _redisHelper.SetCacheAsync(appointmentCacheKey, appointmentCacheDto, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_customer_{appointment.CustomerId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_business_{appointment.BusinessId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_staff_{appointment.StaffId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromMinutes(10));
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
                TimeSpan.FromMinutes(10));

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_business_{appointment.BusinessId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromMinutes(10));

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_staff_{appointment.StaffId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private AppointmentCacheDto MapToCacheDto(Appointment appointment)
        {
            _context.Entry(appointment).Reference(a => a.Customer).Load();
            _context.Entry(appointment).Reference(a => a.Staff).Load();

            if (appointment.AppointmentServices != null)
            {
                _context.Entry(appointment).Collection(a => a.AppointmentServices!).Query()
                    .Include(apptService => apptService.Service).Load();
            }

            // Calculate the total duration based on actual service duration in the appointment
            var totalDuration = appointment.Duration.TotalMinutes;

            return new AppointmentCacheDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? string.Empty,
                CustomerPhone = appointment.Customer?.Phone ?? string.Empty,
                BusinessId = appointment.BusinessId,
                StaffId = appointment.StaffId,
                StaffName = appointment.Staff?.Name ?? string.Empty,
                StaffPhone = appointment.Staff?.Phone ?? string.Empty,
                AppointmentTime = appointment.AppointmentTime,
                Duration = TimeSpan.FromMinutes(totalDuration),  // Use the actual duration from the appointment
                Comment = appointment.Comment ?? string.Empty,
                Status = appointment.Status,
                ServiceIds = appointment.AppointmentServices?.Select(apptService => apptService.ServiceId).ToList() ??
                             new List<int>()
            };
        }


        private Appointment MapFromCacheDto(AppointmentCacheDto dto)
        {
            var services = _context.Services.Where(s => dto.ServiceIds.Contains(s.ServiceId)).ToList();

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
                StaffId = dto.StaffId,
                Staff = new Staff
                {
                    StaffId = dto.StaffId,
                    Name = dto.StaffName,
                    Phone = dto.StaffPhone
                },
                AppointmentTime = dto.AppointmentTime,
                Duration = dto.Duration,
                Status = dto.Status,
                Comment = dto.Comment,
                AppointmentServices = services.Select(service => new AppointmentServiceMapping
                    { ServiceId = service.ServiceId, Service = service }).ToList()
            };
        }

        private DateTime ConvertToLocalTime(DateTime utcTime)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);
        }
    }
}