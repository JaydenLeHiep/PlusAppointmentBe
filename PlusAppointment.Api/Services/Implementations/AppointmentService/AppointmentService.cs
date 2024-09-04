using Hangfire;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Services.Interfaces.EmailUsageService;
using PlusAppointment.Utils.SendingEmail;
using PlusAppointment.Utils.SendingSms;

namespace PlusAppointment.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        //private readonly IAppointmentRepository _appointmentRepository;

        private readonly IAppointmentWriteRepository _appointmentWriteRepository;
        private readonly IAppointmentReadRepository _appointmentReadRepository;
        private readonly IBusinessRepository _businessRepository;

        private readonly EmailService _emailService;
        private readonly SmsTextMagicService _smsTextMagicService;
        private readonly IServicesRepository _servicesRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IEmailUsageService _emailUsageService;

        public AppointmentService(IAppointmentWriteRepository appointmentWriteRepository,
            IAppointmentReadRepository appointmentReadRepository, IBusinessRepository businessRepository,
            EmailService emailService, SmsTextMagicService smsTextMagicService, IServicesRepository servicesRepository,
            IStaffRepository staffRepository, IEmailUsageService emailUsageService)
        {
            _appointmentWriteRepository = appointmentWriteRepository;
            _appointmentReadRepository = appointmentReadRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;
            _smsTextMagicService = smsTextMagicService;
            _servicesRepository = servicesRepository;
            _staffRepository = staffRepository;
            _emailUsageService = emailUsageService;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto?>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentReadRepository.GetAllAppointmentsAsync();
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<AppointmentRetrieveDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentReadRepository.GetAppointmentByIdAsync(id);
            return appointment == null ? null : await MapToDtoAsync(appointment);
        }

        public async Task<bool> AddAppointmentAsync(AppointmentDto appointmentDto)
        {
            var business = await _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            if (business == null)
            {
                throw new ArgumentException("Invalid BusinessId");
            }

            var customer = await _appointmentReadRepository.GetByCustomerIdAsync(appointmentDto.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException("Invalid CustomerId");
            }

            var mappings = new List<AppointmentServiceStaffMapping>();

            foreach (var serviceStaff in appointmentDto.Services)
            {
                var staff = await _staffRepository.GetByIdAsync(serviceStaff.StaffId);
                var service = await _servicesRepository.GetByIdAsync(serviceStaff.ServiceId);

                if (staff == null || service == null)
                {
                    throw new ArgumentException("Invalid ServiceId or StaffId");
                }

                mappings.Add(new AppointmentServiceStaffMapping
                {
                    ServiceId = service.ServiceId,
                    StaffId = staff.StaffId,
                    Service = service,
                    Staff = staff
                });
            }

            var appointment = new Appointment
            {
                CustomerId = appointmentDto.CustomerId,
                Customer = customer,
                BusinessId = appointmentDto.BusinessId,
                Business = business,
                AppointmentTime = appointmentDto.AppointmentTime,
                Duration = TimeSpan.Zero,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comment = appointmentDto.Comment,
                AppointmentServices = mappings
            };

            var errors = new List<string>();

            // Convert appointment time to Vienna local time
            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointmentDto.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            var subject = "Appointment Received";

            // Add the appointment to the database
            try
            {
                await _appointmentWriteRepository.AddAppointmentAsync(appointment);

                // Send email to the customer after successfully saving the appointment
                var bodySms =
                    $"Dear Customer, \n\nThank you for choosing {business.Name}. We have successfully received your appointment request for {appointmentTimeFormatted}. Our team is currently processing it, and we will confirm the details with you shortly. If needed, we will reach out to you via email or phone. \n\nBest regards,\n{business.Name}";

                var emailSent = await _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, bodySms);
                if (emailSent)
                {
                    // Update EmailUsage for customer
                    await _emailUsageService.AddEmailUsageAsync(new EmailUsage
                    {
                        BusinessId = appointmentDto.BusinessId,
                        Year = DateTime.UtcNow.Year,
                        Month = DateTime.UtcNow.Month,
                        EmailCount = 1
                    });
                }
                else
                {
                    Console.WriteLine("Failed to send confirmation email to customer.");
                    errors.Add("Failed to send confirmation email to customer.");
                }

                // Send notification email to the business
                var businessNotificationSubject = "New Appointment Request";
                var businessNotificationBody =
                    $"Customer {customer.Name} has requested an appointment at {appointmentTimeFormatted}. Please review the appointment details and confirm accordingly.";

                var businessEmailSent = await _emailService.SendEmailAsync(business.Email ?? string.Empty,
                    businessNotificationSubject, businessNotificationBody);
                if (businessEmailSent)
                {
                    // Update EmailUsage for business email
                    await _emailUsageService.AddEmailUsageAsync(new EmailUsage
                    {
                        BusinessId = appointmentDto.BusinessId,
                        Year = DateTime.UtcNow.Year,
                        Month = DateTime.UtcNow.Month,
                        EmailCount = 1
                    });
                }
                else
                {
                    Console.WriteLine("Failed to send notification email to business.");
                    errors.Add("Failed to send notification email to business.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save appointment: {ex.Message}");
                errors.Add($"Failed to save appointment: {ex.Message}");
            }

            // Schedule the reminder email 48 hours (2 days) before the appointment
            var bodyEmail =
                $"Dear Customer, \n\nThis is a friendly reminder of your upcoming appointment at {business.Name} scheduled for {appointmentTimeFormatted}. Please ensure you arrive on time. We look forward to seeing you! \n\nBest regards,\n{business.Name}";

            var sendTime = viennaTime.AddDays(-2);

            if (sendTime <= DateTime.UtcNow)
            {
                // If the send time is in the past (less than 48 hours left), send the email immediately
                var reminderEmailSent =
                    await _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, bodyEmail);
                if (reminderEmailSent)
                {
                    // Update EmailUsage for reminder email
                    await _emailUsageService.AddEmailUsageAsync(new EmailUsage
                    {
                        BusinessId = appointmentDto.BusinessId,
                        Year = DateTime.UtcNow.Year,
                        Month = DateTime.UtcNow.Month,
                        EmailCount = 1
                    });
                }
            }
            else
            {
                BackgroundJob.Schedule(
                    () => SendReminderEmail(customer.Email ?? string.Empty, subject, bodyEmail,
                        appointmentDto.BusinessId),
                    new DateTimeOffset(sendTime));
            }

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    // Log the errors or handle them as needed
                }
            }

            return !errors.Any();
        }


        // Separate method for sending the reminder email and updating the email usage
        public async Task SendReminderEmail(string email, string subject, string bodyEmail, int businessId)
        {
            var emailSent = await _emailService.SendEmailAsync(email, subject, bodyEmail);
            if (emailSent)
            {
                await _emailUsageService.AddEmailUsageAsync(new EmailUsage
                {
                    BusinessId = businessId,
                    Year = DateTime.UtcNow.Year,
                    Month = DateTime.UtcNow.Month,
                    EmailCount = 1
                });
            }
        }

        public async Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            await _appointmentWriteRepository.UpdateAppointmentWithServicesAsync(id, updateAppointmentDto);
        }

        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            var appointment = await _appointmentReadRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found");
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;

            var customer = await _appointmentReadRepository.GetByCustomerIdAsync(appointment.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException("Invalid CustomerId");
            }

            var business = await _businessRepository.GetByIdAsync(appointment.BusinessId);
            if (business == null)
            {
                throw new ArgumentException("Invalid BusinessId");
            }

            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            var errors = new List<string>();
            // Convert appointment time to Vienna local time

            var subject = "Appointment Confirmation";
            var bodySms =
                $"Dear Customer, \n\nThank you for choosing {business.Name}. We are pleased to confirm your appointment at {appointmentTimeFormatted}. We look forward to serving you! \n\nBest regards,\n{business.Name}";

            try
            {
                var emailSent = await _emailService.SendEmailAsync(customer.Email ?? string.Empty, subject, bodySms);
                if (emailSent)
                {
                    // Update EmailUsage
                    await _emailUsageService.AddEmailUsageAsync(new EmailUsage
                    {
                        BusinessId = appointment.BusinessId,
                        Year = DateTime.UtcNow.Year,
                        Month = DateTime.UtcNow.Month,
                        EmailCount = 1
                    });
                }
                else
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

            await _appointmentWriteRepository.UpdateAppointmentStatusAsync(appointment);
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            await _appointmentWriteRepository.DeleteAppointmentAsync(id);
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            var appointments = await _appointmentReadRepository.GetAppointmentsByCustomerIdAsync(customerId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetCustomerAppointmentHistoryAsync(int customerId)
        {
            var appointments = await _appointmentReadRepository.GetCustomerAppointmentHistoryAsync(customerId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByBusinessIdAsync(int businessId)
        {
            var appointments = await _appointmentReadRepository.GetAppointmentsByBusinessIdAsync(businessId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<AppointmentRetrieveDto>> GetAppointmentsByStaffIdAsync(int staffId)
        {
            var appointments = await _appointmentReadRepository.GetAppointmentsByStaffIdAsync(staffId);
            var dtoList = new List<AppointmentRetrieveDto>();

            foreach (var appointment in appointments)
            {
                dtoList.Add(await MapToDtoAsync(appointment));
            }

            return dtoList;
        }

        public async Task<IEnumerable<DateTime>> GetNotAvailableTimeSlotsAsync(int staffId, DateTime date)
        {
            return await _appointmentReadRepository.GetNotAvailableTimeSlotsAsync(staffId, date);
        }

        private async Task<AppointmentRetrieveDto> MapToDtoAsync(Appointment appointment)
        {
            var services = new List<ServiceStaffListsRetrieveDto>();

            foreach (var apptService in appointment.AppointmentServices ??
                                        Enumerable.Empty<AppointmentServiceStaffMapping>())
            {
                var category = apptService.Service?.CategoryId != null
                    ? await _appointmentReadRepository.GetServiceCategoryByIdAsync(apptService.Service.CategoryId.Value)
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