using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;
using WebApplication1.Repositories.Interfaces.BusinessRepo;
using WebApplication1.Services.Interfaces.AppointmentService;

namespace WebApplication1.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBusinessRepository _businessRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository, IBusinessRepository businessRepository)
        {
            _appointmentRepository = appointmentRepository;
            _businessRepository = businessRepository;
        }

        public async Task<IEnumerable<AppointmentDto?>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
            return appointments.Select(a => MapToDto(a));
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            return appointment == null ? null : MapToDto(appointment);
        }

        public async Task AddAppointmentAsync(AppointmentDto appointmentDto)
        {
            // Validate the BusinessId
            var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            if (business == null)
            {
                throw new ArgumentException("Invalid BusinessId");
            }

            // Validate the ServiceId
            var services = await _businessRepository.GetServicesByBusinessIdAsync(appointmentDto.BusinessId);
            if (services == null || !services.Any(s => s != null && s.ServiceId == appointmentDto.ServiceId))
            {
                throw new ArgumentException("Invalid ServiceId for the given BusinessId");
            }

            // Validate the StaffId
            var staff = await _businessRepository.GetStaffByBusinessIdAsync(appointmentDto.BusinessId);
            if (staff == null || !staff.Any(s => s != null && s.StaffId == appointmentDto.StaffId))
            {
                throw new ArgumentException("Invalid StaffId for the given BusinessId");
            }

            // Adjust the appointment time by subtracting 2 hours
            var adjustedAppointmentTime = appointmentDto.AppointmentTime.AddHours(-2);
            var timeNow = DateTime.UtcNow;

            // Check if the appointment time is in the past
            if (adjustedAppointmentTime < timeNow)
            {
                throw new InvalidOperationException("Cannot book an appointment in the past.");
            }

            // Check if the staff is available
            var isAvailable = await _appointmentRepository.IsStaffAvailable(
                appointmentDto.StaffId,
                adjustedAppointmentTime,
                appointmentDto.Duration);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The staff is not available at the requested time.");
            }

            var appointment = new Appointment
            {
                CustomerId = appointmentDto.CustomerId,
                BusinessId = appointmentDto.BusinessId,
                ServiceId = appointmentDto.ServiceId,
                StaffId = appointmentDto.StaffId,
                AppointmentTime = DateTime.SpecifyKind(adjustedAppointmentTime, DateTimeKind.Utc),
                Duration = appointmentDto.Duration,
                Status = appointmentDto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _appointmentRepository.AddAppointmentAsync(appointment);
        }


        public async Task UpdateAppointmentAsync(int id, AppointmentDto appointmentDto)
        {
            // Validate the BusinessId
            var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            if (business == null)
            {
                throw new ArgumentException("Invalid BusinessId");
            }

            // Validate the ServiceId
            var services = await _businessRepository.GetServicesByBusinessIdAsync(appointmentDto.BusinessId);
            if (!services.Any(s => s != null && s.ServiceId == appointmentDto.ServiceId))
            {
                throw new ArgumentException("Invalid ServiceId for the given BusinessId");
            }

            // Validate the StaffId
            var staff = await _businessRepository.GetStaffByBusinessIdAsync(appointmentDto.BusinessId);
            if (!staff.Any(s => s != null && s.StaffId == appointmentDto.StaffId))
            {
                throw new ArgumentException("Invalid StaffId for the given BusinessId");
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found");
            }

            appointment.CustomerId = appointmentDto.CustomerId;
            appointment.BusinessId = appointmentDto.BusinessId;
            appointment.ServiceId = appointmentDto.ServiceId;
            appointment.StaffId = appointmentDto.StaffId;
            appointment.AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc);
            appointment.Duration = appointmentDto.Duration;
            appointment.Status = appointmentDto.Status;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAppointmentAsync(appointment);
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            await _appointmentRepository.DeleteAppointmentAsync(id);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByCustomerIdAsync(customerId);
            return appointments.Select(a => MapToDto(a));
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByBusinessIdAsync(businessId);
            return appointments.Select(a => MapToDto(a));
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByStaffIdAsync(staffId);
            return appointments.Select(a => MapToDto(a));
        }

        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? "Unknown Customer Name",
                CustomerPhone = appointment.Customer?.Phone ?? "Unknown Customer Phone",
                BusinessId = appointment.BusinessId,
                BusinessName = appointment.Business?.Name ?? "Unknown Business Name",
                ServiceId = appointment.ServiceId,
                ServiceName = appointment.Service?.Name ?? "Unknown Service Name",
                StaffId = appointment.StaffId,
                StaffName = appointment.Staff?.Name ?? "Unknown Staff Name",
                AppointmentTime = appointment.AppointmentTime,
                Duration = appointment.Duration,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }
    }
}