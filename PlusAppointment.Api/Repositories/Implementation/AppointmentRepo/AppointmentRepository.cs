using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo;
using PlusAppointment.Utils.Redis;
using Npgsql;
using NpgsqlTypes;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;
        private readonly ICustomerRepository _customerRepository;
        private readonly IServicesRepository _servicesRepository;
        private readonly IStaffRepository _staffRepository;

        public AppointmentRepository(ApplicationDbContext context, RedisHelper redisHelper,
            ICustomerRepository customerRepository, IServicesRepository servicesRepository,
            IStaffRepository staffRepository)
        {
            _context = context;
            _redisHelper = redisHelper;
            _customerRepository = customerRepository;
            _servicesRepository = servicesRepository;
            _staffRepository = staffRepository;
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

            appointmentCacheDto = MapToCacheDto(appointment);
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDto, TimeSpan.FromMinutes(10));
            return appointment;
        }

        // public async Task<bool> IsStaffAvailable(int staffId, DateTime appointmentTime, TimeSpan duration)
        // {
        //     var endTime = appointmentTime.Add(duration);
        //
        //     var appointments = await _context.Appointments
        //         .Include(a => a.AppointmentServices)
        //         .Where(a => a.AppointmentServices != null &&
        //                     a.AppointmentServices.Any(apptService => apptService.StaffId == staffId))
        //         .ToListAsync();
        //
        //     foreach (var appointment in appointments)
        //     {
        //         var existingAppointmentEndTime = appointment.AppointmentTime.Add(appointment.Duration);
        //         if (appointment.AppointmentTime < endTime && existingAppointmentEndTime > appointmentTime)
        //         {
        //             return false;
        //         }
        //     }
        //
        //     return true;
        // }

        public async Task<Customer?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }

        private async Task RefreshRelatedCachesAsync(Appointment appointment)
        {
            // Refresh Business Cache
            string businessCacheKey = $"appointments_business_{appointment.BusinessId}";
            var startOfTodayUtc = GetStartOfTodayUtc();
            var businessAppointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.BusinessId == appointment.BusinessId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            var businessCacheDtos = businessAppointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(businessCacheKey, businessCacheDtos, TimeSpan.FromMinutes(10));

            // Refresh Customer Cache
            string customerCacheKey = $"appointments_customer_{appointment.CustomerId}";
            var customerAppointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.CustomerId == appointment.CustomerId && a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            var customerCacheDtos = customerAppointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(customerCacheKey, customerCacheDtos, TimeSpan.FromMinutes(10));

            // Refresh Staff Cache for each staff involved in the appointment
            if (appointment.AppointmentServices != null)
            {
                foreach (var service in appointment.AppointmentServices)
                {
                    string staffCacheKey = $"appointments_staff_{service.StaffId}";
                    var staffAppointments = await _context.Appointments
                        .Include(a => a.Customer)
                        .Include(a => a.Business)
                        .Include(a => a.AppointmentServices)!
                        .ThenInclude(apptService => apptService.Service)
                        .Include(a => a.AppointmentServices)!
                        .ThenInclude(apptService => apptService.Staff)
                        .Where(a => a.AppointmentServices.Any(apptService => apptService.StaffId == service.StaffId) && a.AppointmentTime >= startOfTodayUtc)
                        .ToListAsync();

                    var staffCacheDtos = staffAppointments.Select(MapToCacheDto).ToList();
                    await _redisHelper.SetCacheAsync(staffCacheKey, staffCacheDtos, TimeSpan.FromMinutes(10));
                }
            }
        }


        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert the appointment
                var appointmentSql = @"
                INSERT INTO appointments (customer_id, business_id, appointment_time, duration, status, created_at, updated_at, comment)
                VALUES (@CustomerId, @BusinessId, @AppointmentTime, @Duration, @Status, @CreatedAt, @UpdatedAt, @Comment)
                RETURNING appointment_id;";

                await using var cmd = new NpgsqlCommand(appointmentSql, connection, transaction);
                cmd.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
                cmd.Parameters.AddWithValue("@BusinessId", appointment.BusinessId);
                cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                cmd.Parameters.AddWithValue("@Duration", NpgsqlDbType.Interval, appointment.Duration);
                cmd.Parameters.AddWithValue("@Status", appointment.Status);
                cmd.Parameters.AddWithValue("@CreatedAt", appointment.CreatedAt);
                cmd.Parameters.AddWithValue("@UpdatedAt", appointment.UpdatedAt);
                cmd.Parameters.AddWithValue("@Comment", (object)appointment.Comment ?? DBNull.Value);

                var appointmentId = (int)await cmd.ExecuteScalarAsync();
                appointment.AppointmentId = appointmentId;

                if (appointment.AppointmentServices != null && appointment.AppointmentServices.Any())
                {
                    // Insert AppointmentServiceStaffMappings
                    var mappingSql = @"
                    INSERT INTO appointment_services_staffs (appointment_id, service_id, staff_id)
                    VALUES (@AppointmentId, @ServiceId, @StaffId)";

                    await using var mappingCmd = new NpgsqlCommand(mappingSql, connection, transaction);
                    mappingCmd.Parameters.Add("@AppointmentId", NpgsqlDbType.Integer);
                    mappingCmd.Parameters.Add("@ServiceId", NpgsqlDbType.Integer);
                    mappingCmd.Parameters.Add("@StaffId", NpgsqlDbType.Integer);

                    foreach (var mapping in appointment.AppointmentServices)
                    {
                        mappingCmd.Parameters["@AppointmentId"].Value = appointmentId;
                        mappingCmd.Parameters["@ServiceId"].Value = mapping.ServiceId;
                        mappingCmd.Parameters["@StaffId"].Value = mapping.StaffId;
                        await mappingCmd.ExecuteNonQueryAsync();
                    }

                    // Calculate and update total duration
                    var serviceIds = appointment.AppointmentServices.Select(mapping => mapping.ServiceId).Distinct()
                        .ToList();
                    var services = await _context.Services
                        .Where(s => serviceIds.Contains(s.ServiceId))
                        .ToDictionaryAsync(s => s.ServiceId, s => s.Duration);

                    var totalDuration = appointment.AppointmentServices
                        .Select(mapping => services[mapping.ServiceId])
                        .Aggregate(TimeSpan.Zero, (sum, next) => sum.Add(next));

                    var updateDurationSql = @"
                    UPDATE appointments
                    SET duration = @Duration
                    WHERE appointment_id = @AppointmentId";

                    await using var updateCmd = new NpgsqlCommand(updateDurationSql, connection, transaction);
                    updateCmd.Parameters.AddWithValue("@Duration", NpgsqlDbType.Interval, totalDuration);
                    updateCmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    await updateCmd.ExecuteNonQueryAsync();

                    appointment.Duration = totalDuration;
                }

                await transaction.CommitAsync();
                await UpdateAppointmentCacheAsync(appointment);
                await RefreshRelatedCachesAsync(appointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }





        public async Task UpdateAppointmentWithServicesAsync(int appointmentId,
            UpdateAppointmentDto updateAppointmentDto)
        {
            using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Update appointment details
                var updateAppointmentSql = @"
        UPDATE appointments 
        SET appointment_time = @AppointmentTime, 
            comment = @Comment, 
            updated_at = @UpdatedAt
        WHERE appointment_id = @AppointmentId";

                using var updateAppointmentCmd = new NpgsqlCommand(updateAppointmentSql, connection, transaction);
                updateAppointmentCmd.Parameters.AddWithValue("AppointmentTime", updateAppointmentDto.AppointmentTime);
                updateAppointmentCmd.Parameters.AddWithValue("Comment",
                    updateAppointmentDto.Comment ?? (object)DBNull.Value);
                updateAppointmentCmd.Parameters.AddWithValue("UpdatedAt", DateTime.UtcNow);
                updateAppointmentCmd.Parameters.AddWithValue("AppointmentId", appointmentId);

                await updateAppointmentCmd.ExecuteNonQueryAsync();

                // Remove existing service mappings
                var deleteServicesSql = "DELETE FROM appointment_services_staffs WHERE appointment_id = @AppointmentId";
                using var deleteServicesCmd = new NpgsqlCommand(deleteServicesSql, connection, transaction);
                deleteServicesCmd.Parameters.AddWithValue("AppointmentId", appointmentId);
                await deleteServicesCmd.ExecuteNonQueryAsync();

                // Insert new service mappings and calculate total duration
                var insertServiceSql = @"
        INSERT INTO appointment_services_staffs (appointment_id, service_id, staff_id)
        VALUES (@AppointmentId, @ServiceId, @StaffId)";

                using var insertServiceCmd = new NpgsqlCommand(insertServiceSql, connection, transaction);
                insertServiceCmd.Parameters.Add("AppointmentId", NpgsqlDbType.Integer);
                insertServiceCmd.Parameters.Add("ServiceId", NpgsqlDbType.Integer);
                insertServiceCmd.Parameters.Add("StaffId", NpgsqlDbType.Integer);

                TimeSpan totalDuration = TimeSpan.Zero;
                var updatedAppointmentServices = new List<AppointmentServiceStaffMapping>();

                foreach (var serviceDto in updateAppointmentDto.Services)
                {
                    insertServiceCmd.Parameters["AppointmentId"].Value = appointmentId;
                    insertServiceCmd.Parameters["ServiceId"].Value = serviceDto.ServiceId;
                    insertServiceCmd.Parameters["StaffId"].Value = serviceDto.StaffId;
                    await insertServiceCmd.ExecuteNonQueryAsync();

                    // Fetch full service and staff details
                    var service = await _servicesRepository.GetByIdAsync(serviceDto.ServiceId);
                    var staff = await _staffRepository.GetByIdAsync(serviceDto.StaffId);

                    if (service == null || staff == null)
                    {
                        throw new ArgumentException("Invalid ServiceId or StaffId");
                    }

                    updatedAppointmentServices.Add(new AppointmentServiceStaffMapping
                    {
                        ServiceId = service.ServiceId,
                        Service = service, // Attach full Service entity
                        StaffId = staff.StaffId,
                        Staff = staff // Attach full Staff entity
                    });

                    // Calculate duration
                    var serviceDuration = service.Duration;
                    totalDuration += serviceDto.UpdatedDuration ?? serviceDuration;
                }

                // Update appointment duration
                var updateDurationSql =
                    "UPDATE appointments SET duration = @Duration WHERE appointment_id = @AppointmentId";
                using var updateDurationCmd = new NpgsqlCommand(updateDurationSql, connection, transaction);
                updateDurationCmd.Parameters.AddWithValue("Duration", totalDuration);
                updateDurationCmd.Parameters.AddWithValue("AppointmentId", appointmentId);
                await updateDurationCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                // Fetch updated appointment with customer, business, service, and staff details for cache update
                var updatedAppointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Business)
                    .Include(a => a.AppointmentServices)
                    .ThenInclude(apptService => apptService.Service)
                    .Include(a => a.AppointmentServices)
                    .ThenInclude(apptService => apptService.Staff)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (updatedAppointment != null)
                {
                    // Attach the updated services to the appointment
                    updatedAppointment.AppointmentServices = updatedAppointmentServices;

                    // Ensure customer details are fully loaded
                    var customer = await _customerRepository.GetCustomerByIdAsync(updatedAppointment.CustomerId);
                    if (customer != null)
                    {
                        updatedAppointment.Customer = customer; // Attach full Customer entity
                    }

                    // Update caches related to the appointment
                    await UpdateAppointmentCacheAsync(updatedAppointment);

                    // Refresh the relevant caches
                    await RefreshRelatedCachesAsync(updatedAppointment);
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task UpdateAppointmentStatusAsync(Appointment appointment)
        {
            _context.Entry(appointment).Property(a => a.Status).IsModified = true;
            _context.Entry(appointment).Property(a => a.UpdatedAt).IsModified = true;

            await _context.SaveChangesAsync();

            // Update the cache with the new status
            await UpdateAppointmentCacheAsync(appointment);

            // Refresh the relevant caches
           
            await RefreshRelatedCachesAsync(appointment);
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
            // Refresh the relevant caches
            await RefreshRelatedCachesAsync(appointment);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            string cacheKey = $"appointments_customer_{customerId}";
            var cachedAppointments = await _redisHelper.GetCacheAsync<List<AppointmentCacheDto>>(cacheKey);

            if (cachedAppointments != null && cachedAppointments.Any())
            {
                return cachedAppointments.Select(dto => MapFromCacheDto(dto));
            }

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

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetCustomerAppointmentHistoryAsync(int customerId)
        {
            string cacheKey = $"appointments_customer_{customerId}_history";
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
                .Where(a => a.CustomerId == customerId)
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
                return cachedAppointments.Select(dto => MapFromCacheDto(dto));
            }

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
                return cachedAppointments.Select(dto => MapFromCacheDto(dto));
            }

            var startOfTodayUtc = GetStartOfTodayUtc();

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Business)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Service)
                .Include(a => a.AppointmentServices)!
                .ThenInclude(apptService => apptService.Staff)
                .Where(a => a.AppointmentServices != null &&
                            a.AppointmentServices.Any(apptService => apptService.StaffId == staffId) &&
                            a.AppointmentTime >= startOfTodayUtc)
                .ToListAsync();

            var appointmentCacheDtos = appointments.Select(MapToCacheDto).ToList();
            await _redisHelper.SetCacheAsync(cacheKey, appointmentCacheDtos, TimeSpan.FromMinutes(10));

            return appointments;
        }

        private async Task UpdateAppointmentCacheAsync(Appointment appointment)
        {
            var appointmentCacheDto = MapToCacheDto(appointment);
            var appointmentCacheKey = $"appointment_{appointment.AppointmentId}";

            // Set cache with 10-minute expiration
            await _redisHelper.SetCacheAsync(appointmentCacheKey, appointmentCacheDto, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_customer_{appointment.CustomerId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromMinutes(10)); // Set list cache with 10-minute expiration

            await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                $"appointments_business_{appointment.BusinessId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    list.Add(appointmentCacheDto);
                    return list;
                },
                TimeSpan.FromMinutes(10)); // Set list cache with 10-minute expiration

            if (appointment.AppointmentServices?.Any() == true)
            {
                foreach (var service in appointment.AppointmentServices)
                {
                    await _redisHelper.UpdateListCacheAsync<AppointmentCacheDto>(
                        $"appointments_staff_{service.StaffId}",
                        list =>
                        {
                            list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                            list.Add(appointmentCacheDto);
                            return list;
                        },
                        TimeSpan.FromMinutes(10)); // Set list cache with 10-minute expiration
                }
            }
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
                TimeSpan.FromMinutes(10)); // Set list cache with 10-minute expiration

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_business_{appointment.BusinessId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromMinutes(10)); // Set list cache with 10-minute expiration

            await _redisHelper.RemoveFromListCacheAsync<AppointmentCacheDto>(
                $"appointments_staff_{appointment.AppointmentServices?.FirstOrDefault()?.StaffId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromMinutes(10)); // Set list cache with 10-minute expiration
        }


        private AppointmentCacheDto MapToCacheDto(Appointment appointment)
        {
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

        public async Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date)
        {
            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var appointments = await _context.AppointmentServiceStaffs
                .Where(ass => ass.StaffId == staffId && ass.Appointment.AppointmentTime.Date == utcDate)
                .Select(ass => new
                {
                    ass.Appointment.AppointmentTime,
                    ass.Appointment.Duration
                })
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var notAvailableTimeSlots = new List<DateTime>();

            foreach (var appointment in appointments)
            {
                var appointmentStart = appointment.AppointmentTime;
                var appointmentEnd = appointment.AppointmentTime.Add(appointment.Duration);

                var currentTimeSlot = appointmentStart;

                while (currentTimeSlot < appointmentEnd)
                {
                    notAvailableTimeSlots.Add(currentTimeSlot);
                    currentTimeSlot = currentTimeSlot.AddMinutes(15);
                }
            }

            return notAvailableTimeSlots;
        }

        public async Task<ServiceCategory?> GetServiceCategoryByIdAsync(int categoryId)
        {
            return await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.CategoryId == categoryId);
        }
    }
}