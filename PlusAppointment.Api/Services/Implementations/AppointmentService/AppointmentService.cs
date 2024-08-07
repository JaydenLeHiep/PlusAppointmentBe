using Hangfire;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Utils.SendingEmail;
using PlusAppointment.Utils.SendingSms;

namespace PlusAppointment.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly EmailService _emailService;
        //private readonly SmsService _smsService;
        private readonly SmsTextMagicService _smsTextMagicService;

        public AppointmentService(IAppointmentRepository appointmentRepository, IBusinessRepository businessRepository,
            EmailService emailService, SmsTextMagicService smsTextMagicService)
        {
            _appointmentRepository = appointmentRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;
            //_smsService = smsService;
            _smsTextMagicService = smsTextMagicService;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto?>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
            return appointments.Select(a => MapToDto(a));
        }

        public async Task<AppointmentRetrieveDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            return appointment == null ? null : MapToDto(appointment);
        }

        private DateTime ConvertToLocalTime(DateTime utcTime)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);
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
            var validServices = services.Where(s => s != null && appointmentDto.ServiceIds.Contains(s.ServiceId))
                .ToList();
            if (validServices == null || !validServices.Any())
            {
                throw new ArgumentException("Invalid ServiceIds for the given BusinessId");
            }

            // Check if any service in validServices is null
            if (validServices.Any(service => service == null))
            {
                return false;
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
            var totalDuration =
                validServices.Aggregate(TimeSpan.Zero, (sum, next) => sum.Add(next?.Duration ?? TimeSpan.Zero));
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
                AppointmentTime = DateTime.SpecifyKind(appointmentDto.AppointmentTime, DateTimeKind.Utc),
                Duration = totalDuration,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comment = appointmentDto.Comment,
                AppointmentServices = validServices
                    .Select(service => new AppointmentServiceMapping
                    {
                        ServiceId = service!.ServiceId
                    }).ToList()
            };

            // Try to send the email first
            var localTimeAppointment = ConvertToLocalTime(appointmentDto.AppointmentTime);
            var subject = "Appointment Confirmation";
            var bodySms =
                $"Plus Appointment. Your appointment at {business.Name} for {localTimeAppointment} has been confirmed.";

            // Commented out the email sending code
            // var emailSent = await _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, body);
            // if (!emailSent)
            // {
            //     return false;
            // }

            // Attempt to send an SMS notification instead of email
            var smsSent = await _smsTextMagicService.SendSmsAsync(customer.Phone ?? string.Empty, bodySms);
            if (!smsSent)
            {
                return false;
            }
            
            var bodyEmail =
                $"Plus Appointment. Your appointment at {business.Name} tomorrow.";
            // Save the appointment if the SMS was sent successfully
            await _appointmentRepository.AddAppointmentAsync(appointment);

            // Schedule SMS reminder
            var sendTime = localTimeAppointment.AddDays(-1);
            BackgroundJob.Schedule(
                () => _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, bodyEmail),
                sendTime);

            return true;
        }


        public async Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            // No need to validate services here as it's handled on the frontend
    
            // Update the appointment and services through the repository
            await _appointmentRepository.UpdateAppointmentWithServicesAsync(id, updateAppointmentDto);
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

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByCustomerIdAsync(customerId);
            return appointments.Select(a => MapToDto(a));
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByBusinessIdAsync(businessId);
            return appointments.Select(a => MapToDto(a));
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByStaffIdAsync(staffId);
            return appointments.Select(a => MapToDto(a));
        }


        private AppointmentRetrieveDto MapToDto(Appointment appointment)
        {
            var services = appointment.AppointmentServices?
                .Select(apptService => new ServiceListsRetrieveDto
                {
                    ServiceId = apptService.ServiceId,
                    Name = apptService.Service?.Name ?? "Unknown Service Name",
                    Duration = apptService.Service?.Duration ?? TimeSpan.Zero
                }).ToList() ?? new List<ServiceListsRetrieveDto>();

            var totalDuration =
                services.Sum(service => service.Duration.HasValue ? service.Duration.Value.TotalMinutes : 0);

            return new AppointmentRetrieveDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? "Unknown Customer Name",
                CustomerPhone = appointment.Customer?.Phone ?? "Unknown Customer Phone",
                CustomerEmail = appointment.Customer?.Email ?? "Unknown Customer Email",
                BusinessId = appointment.BusinessId,
                BusinessName = appointment.Business?.Name ?? "Unknown Business Name",
                StaffId = appointment.StaffId,
                StaffName = appointment.Staff?.Name ?? "Unknown Staff Name",
                AppointmentTime = appointment.AppointmentTime,
                Duration = TimeSpan.FromMinutes(totalDuration),
                Status = appointment.Status,
                Comment = appointment.Comment,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                Services = services
            };
        }
    }
}