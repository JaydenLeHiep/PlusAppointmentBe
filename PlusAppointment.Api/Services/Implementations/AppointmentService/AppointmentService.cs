using Hangfire;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using WebApplication1.Repositories.Interfaces.AppointmentRepo;
using WebApplication1.Repositories.Interfaces.BusinessRepo;
using WebApplication1.Services.Interfaces.AppointmentService;
using WebApplication1.Utils.SendingEmail;
using WebApplication1.Utils.SendingSms;

namespace WebApplication1.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        public AppointmentService(IAppointmentRepository appointmentRepository, IBusinessRepository businessRepository, EmailService emailService, SmsService smsService)
        {
            _appointmentRepository = appointmentRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;
            _smsService = smsService;
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

        public async Task<bool> AddAppointmentAsync(AppointmentDto appointmentDto)
        {
            // Validate the BusinessId
            var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            if (business == null)
            {
                throw new ArgumentException("Invalid BusinessId");
            }

            // Validate the StaffId
            var staff = await _businessRepository.GetStaffByBusinessIdAsync(appointmentDto.BusinessId);
            if (staff == null || !staff.Any(s => s != null && s.StaffId == appointmentDto.StaffId))
            {
                throw new ArgumentException("Invalid StaffId for the given BusinessId");
            }

            // Validate Services
            var services = await _businessRepository.GetServicesByBusinessIdAsync(appointmentDto.BusinessId);
            var validServices = services.Where(s => s != null && appointmentDto.ServiceIds.Contains(s.ServiceId)).ToList();
            if (!validServices.Any())
            {
                throw new ArgumentException("Invalid ServiceIds for the given BusinessId");
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
            var totalDuration = validServices.Aggregate(TimeSpan.Zero, (sum, next) => sum.Add(next.Duration));
            var isAvailable = await _appointmentRepository.IsStaffAvailable(
                appointmentDto.StaffId,
                adjustedAppointmentTime,
                totalDuration);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The staff is not available at the requested time.");
            }

            // Get the customer details
            var customer = await _appointmentRepository.GetByCustomerIdAsync(appointmentDto.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException("Invalid CustomerId");
            }

            var appointment = new Appointment
            {
                CustomerId = appointmentDto.CustomerId,
                BusinessId = appointmentDto.BusinessId,
                StaffId = appointmentDto.StaffId,
                AppointmentTime = DateTime.SpecifyKind(adjustedAppointmentTime, DateTimeKind.Utc),
                Duration = totalDuration,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comment = appointmentDto.Comment,
                AppointmentServices = validServices.Select(service => new AppointmentServiceMapping
                {
                    ServiceId = service.ServiceId
                }).ToList()
            };

            // Try to send the email first
            var subject = "Appointment Confirmation";
            var body = $"Your appointment for {appointmentDto.AppointmentTime} has been confirmed.";
            var emailSent = await _emailService.SendEmailAsync(customer.Email, subject, body);

            if (!emailSent)
            {
                return false;
            }

            // Save the appointment if the email was sent successfully
            await _appointmentRepository.AddAppointmentAsync(appointment);

            // Schedule SMS reminder
            var sendTime = appointment.AppointmentTime.AddDays(-1);
            BackgroundJob.Schedule(() => _smsService.SendSmsAsync(customer.Phone, $"Reminder: You have an appointment on {appointmentDto.AppointmentTime}."), sendTime);

            return true;
        }

        public async Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            // Validate Services
            var services = await _businessRepository.GetServicesByBusinessIdAsync(updateAppointmentDto.BusinessId);
            var validServices = services.Where(s => updateAppointmentDto.ServiceIds.Contains(s.ServiceId)).ToList();
            if (!validServices.Any())
            {
                throw new ArgumentException("Invalid ServiceIds for the given BusinessId");
            }

            var totalDuration = validServices.Aggregate(TimeSpan.Zero, (sum, next) => sum.Add(next.Duration));

            // Update the appointment and services through the repository
            await _appointmentRepository.UpdateAppointmentWithServicesAsync(id, updateAppointmentDto, validServices, totalDuration);
        }



        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found");
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAppointmentStatusAsync(appointment);
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
            var serviceIds = appointment.AppointmentServices?.Select(apptService => apptService.ServiceId).ToList() ?? new List<int>();
            var totalDuration = appointment.AppointmentServices?.Sum(apptService => apptService.Service?.Duration.TotalMinutes ?? 0) ?? 0;

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? "Unknown Customer Name",
                CustomerPhone = appointment.Customer?.Phone ?? "Unknown Customer Phone",
                CustomerEmail = appointment.Customer?.Email?? "Unknown Customer Email",
                BusinessId = appointment.BusinessId,
                BusinessName = appointment.Business?.Name ?? "Unknown Business Name",
                StaffId = appointment.StaffId,
                StaffName = appointment.Staff?.Name ?? "Unknown Staff Name",
                AppointmentTime = appointment.AppointmentTime,
                Duration = TimeSpan.FromMinutes(totalDuration),
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                ServiceIds = serviceIds
            };
        }
    }
}
