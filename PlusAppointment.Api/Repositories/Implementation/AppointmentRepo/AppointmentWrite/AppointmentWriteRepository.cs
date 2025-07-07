using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
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

        private DateTime GetStartOfTodayUtc()
        {
            return DateTime.UtcNow.Date;
        }
        

        public async Task AddAppointmentAsync(Appointment appointment)
        {
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