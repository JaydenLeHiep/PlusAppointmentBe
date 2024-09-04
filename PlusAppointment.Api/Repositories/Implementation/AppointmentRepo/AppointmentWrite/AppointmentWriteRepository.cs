using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Utils.Redis;

using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;

namespace PlusAppointment.Repositories.Implementation.AppointmentRepo.AppointmentWrite
{
    public class AppointmentWriteRepository : IAppointmentWriteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;
        private readonly ICustomerRepository _customerRepository;
        private readonly IServicesRepository _servicesRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IAppointmentReadRepository _appointmentReadRepository;
        public AppointmentWriteRepository(ApplicationDbContext context, RedisHelper redisHelper,
            ICustomerRepository customerRepository, IServicesRepository servicesRepository,
            IStaffRepository staffRepository, IAppointmentReadRepository appointmentReadRepository)
        {
            _context = context;
            _redisHelper = redisHelper;
            _customerRepository = customerRepository;
            _servicesRepository = servicesRepository;
            _staffRepository = staffRepository;
            _appointmentReadRepository = appointmentReadRepository;
        }

        private DateTime GetStartOfTodayUtc()
        {
            return DateTime.UtcNow.Date;
        }

        private async Task CheckAndIncrementAppointmentLimitAsync(int customerId)
        {
            string key = $"appointments_customerId:{customerId}:{DateTime.UtcNow:yyyy-MM-dd}";
            var currentCount = await _redisHelper.GetDecimalCacheAsync(key) ?? 0;

            if (currentCount >= 2)
            {
                throw new InvalidOperationException("You have reached the maximum number of appointments for a day.");
            }

            await _redisHelper.SetDecimalCacheAsync(key, currentCount + 1);

            if (currentCount == 0)
            {
                var expirationTime = DateTime.UtcNow.AddDays(1).Date - DateTime.UtcNow;
                await _redisHelper.SetDecimalCacheAsync(key, currentCount + 1, expirationTime);
            }
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await CheckAndIncrementAppointmentLimitAsync(appointment.CustomerId);

            await using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
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

                    var serviceIds = appointment.AppointmentServices.Select(mapping => mapping.ServiceId).Distinct()
                        .ToList();
                    var services = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId))
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
                await RefreshRelatedCachesAsync(appointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAppointmentWithServicesAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto)
        {
            using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var updateAppointmentSql = @"
                    UPDATE appointments 
                    SET appointment_time = @AppointmentTime, 
                        comment = @Comment, 
                        updated_at = @UpdatedAt
                    WHERE appointment_id = @AppointmentId";

                using var updateAppointmentCmd = new NpgsqlCommand(updateAppointmentSql, connection, transaction);
                updateAppointmentCmd.Parameters.AddWithValue("AppointmentTime", updateAppointmentDto.AppointmentTime);
                updateAppointmentCmd.Parameters.AddWithValue("Comment", updateAppointmentDto.Comment ?? (object)DBNull.Value);
                updateAppointmentCmd.Parameters.AddWithValue("UpdatedAt", DateTime.UtcNow);
                updateAppointmentCmd.Parameters.AddWithValue("AppointmentId", appointmentId);

                await updateAppointmentCmd.ExecuteNonQueryAsync();

                var deleteServicesSql = "DELETE FROM appointment_services_staffs WHERE appointment_id = @AppointmentId";
                using var deleteServicesCmd = new NpgsqlCommand(deleteServicesSql, connection, transaction);
                deleteServicesCmd.Parameters.AddWithValue("AppointmentId", appointmentId);
                await deleteServicesCmd.ExecuteNonQueryAsync();

                var insertServiceSql = @"
                    INSERT INTO appointment_services_staffs (appointment_id, service_id, staff_id)
                    VALUES (@AppointmentId, @ServiceId, @StaffId)";

                using var insertServiceCmd = new NpgsqlCommand(insertServiceSql, connection, transaction);
                insertServiceCmd.Parameters.Add("AppointmentId", NpgsqlDbType.Integer);
                insertServiceCmd.Parameters.Add("ServiceId", NpgsqlDbType.Integer);
                insertServiceCmd.Parameters.Add("StaffId", NpgsqlDbType.Integer);

                TimeSpan totalDuration = TimeSpan.Zero;

                foreach (var serviceDto in updateAppointmentDto.Services)
                {
                    insertServiceCmd.Parameters["AppointmentId"].Value = appointmentId;
                    insertServiceCmd.Parameters["ServiceId"].Value = serviceDto.ServiceId;
                    insertServiceCmd.Parameters["StaffId"].Value = serviceDto.StaffId;
                    await insertServiceCmd.ExecuteNonQueryAsync();

                    var service = await _servicesRepository.GetByIdAsync(serviceDto.ServiceId);
                    var serviceDuration = service.Duration;
                    totalDuration += serviceDto.UpdatedDuration ?? serviceDuration;
                }

                var updateDurationSql =
                    "UPDATE appointments SET duration = @Duration WHERE appointment_id = @AppointmentId";
                using var updateDurationCmd = new NpgsqlCommand(updateDurationSql, connection, transaction);
                updateDurationCmd.Parameters.AddWithValue("Duration", totalDuration);
                updateDurationCmd.Parameters.AddWithValue("AppointmentId", appointmentId);
                await updateDurationCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                // Fetch the updated appointment using the read repository
                var updatedAppointment = await _appointmentReadRepository.GetAppointmentByIdAsync(appointmentId);
                if (updatedAppointment != null)
                {
                    await RefreshRelatedCachesAsync(updatedAppointment);
                }
            }
            catch
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
            await RefreshRelatedCachesAsync(appointment);
        }

        private async Task RefreshRelatedCachesAsync(Appointment appointment)
        {
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
                        .Where(a => a.AppointmentServices.Any(apptService => apptService.StaffId == service.StaffId) &&
                                    a.AppointmentTime >= startOfTodayUtc)
                        .ToListAsync();

                    var staffCacheDtos = staffAppointments.Select(MapToCacheDto).ToList();
                    await _redisHelper.SetCacheAsync(staffCacheKey, staffCacheDtos, TimeSpan.FromMinutes(10));
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
                $"appointments_staff_{appointment.AppointmentServices?.FirstOrDefault()?.StaffId}",
                list =>
                {
                    list.RemoveAll(a => a.AppointmentId == appointment.AppointmentId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
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
    }
}
