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
        private readonly SmsTextMagicService _smsTextMagicService;

        public AppointmentService(IAppointmentRepository appointmentRepository, IBusinessRepository businessRepository,
            EmailService emailService, SmsTextMagicService smsTextMagicService)
        {
            _appointmentRepository = appointmentRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;
            _smsTextMagicService = smsTextMagicService;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto?>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<AppointmentRetrieveDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            return appointment == null ? null : await MapToDtoAsync(appointment);
        }

        public async Task<bool> AddAppointmentAsync(AppointmentDto appointmentDto)
        {
            var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            if (business == null)
            {
                throw new ArgumentException("Invalid BusinessId");
            }

            var customer = await _appointmentRepository.GetByCustomerIdAsync(appointmentDto.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException("Invalid CustomerId");
            }

            var mappings = appointmentDto.Services.Select(serviceStaff => new AppointmentServiceStaffMapping
            {
                ServiceId = serviceStaff.ServiceId,
                StaffId = serviceStaff.StaffId
            }).ToList();

            var appointment = new Appointment
            {
                CustomerId = appointmentDto.CustomerId,
                BusinessId = appointmentDto.BusinessId,
                AppointmentTime = appointmentDto.AppointmentTime,
                Duration = TimeSpan.Zero,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comment = appointmentDto.Comment,
                AppointmentServices = mappings
            };

            var errors = new List<string>();

            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            var subject = "Appointment Confirmation";
            var bodySms = 
                $"Dear Customer, \n\nThank you for choosing {business.Name}. We are pleased to confirm your appointment at {appointmentTimeFormatted}. We look forward to serving you! \n\nBest regards,\n{business.Name}";

            try
            {
                var emailSent = await _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, bodySms);
                if (!emailSent)
                {
                    Console.WriteLine("Failed to send confirmation email.");
                    errors.Add("Failed to send confirmation email.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send confirmation email.");
                errors.Add($"Error sending confirmation email: {ex.Message}");
            }

            await _appointmentRepository.AddAppointmentAsync(appointment);

            var bodyEmail = 
                $"Dear Customer, \n\nThis is a friendly reminder of your upcoming appointment at {business.Name} scheduled for {appointmentTimeFormatted}. Please ensure you arrive on time. We look forward to seeing you! \n\nBest regards,\n{business.Name}";

            var sendTime = appointmentDto.AppointmentTime.AddDays(-1);
            BackgroundJob.Schedule(
                () => _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, bodyEmail),
                sendTime);

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    // Log the errors or handle them as needed
                }
            }

            return true;
        }

        public async Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
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
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetCustomerAppointmentHistoryAsync(int customerId)
        {
            var appointments = await _appointmentRepository.GetCustomerAppointmentHistoryAsync(customerId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByBusinessIdAsync(businessId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByStaffIdAsync(staffId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date)
        {
            return await _appointmentRepository.GetNotAvailableTimeSlotsAsync(staffId, date);
        }

        private async Task<AppointmentRetrieveDto> MapToDtoAsync(Appointment appointment)
        {
            var services = new List<ServiceStaffListsRetrieveDto>();

            foreach (var apptService in appointment.AppointmentServices ?? Enumerable.Empty<AppointmentServiceStaffMapping>())
            {
                var category = apptService.Service?.CategoryId != null 
                    ? await _appointmentRepository.GetServiceCategoryByIdAsync(apptService.Service.CategoryId.Value)
                    : null;

                var serviceDto = new ServiceStaffListsRetrieveDto
                {
                    ServiceId = apptService.ServiceId,
                    Name = apptService.Service?.Name ?? "Unknown Service Name",
                    Description = apptService.Service?.Description,
                    Duration = apptService.Service?.Duration ?? TimeSpan.Zero,
                    Price = apptService.Service?.Price,
                    StaffId = apptService.StaffId,
                    StaffName = apptService.Staff?.Name ?? "Unknown Staff Name",
                    CategoryId = apptService.Service?.CategoryId,
                    CategoryName = category?.Name ?? "Unknown Category"
                };

                services.Add(serviceDto);
            }

            return new AppointmentRetrieveDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? "Unknown Customer Name",
                CustomerPhone = appointment.Customer?.Phone ?? "Unknown Customer Phone",
                CustomerEmail = appointment.Customer?.Email ?? "Unknown Customer Email",
                BusinessId = appointment.BusinessId,
                BusinessName = appointment.Business?.Name ?? "Unknown Business Name",
                AppointmentTime = appointment.AppointmentTime,
                Duration = appointment.Duration,
                Status = appointment.Status,
                Comment = appointment.Comment ?? "No comment",
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                Services = services
            };
        }
    }
}
