using Hangfire;
using Newtonsoft.Json;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Services.Interfaces.EmailUsageService;
using PlusAppointment.Services.Interfaces.NotificationService;
using PlusAppointment.Utils.SendingEmail;
using PlusAppointment.Utils.SendingSms;
using PlusAppointment.Utils.SQS;


namespace PlusAppointment.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        //private readonly IAppointmentRepository _appointmentRepository;

        private readonly IAppointmentWriteRepository _appointmentWriteRepository;
        private readonly IAppointmentReadRepository _appointmentReadRepository;
        private readonly IBusinessRepository _businessRepository;

        private readonly IEmailService _emailService;
        private readonly SmsTextMagicService _smsTextMagicService;
        private readonly IServicesRepository _servicesRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IEmailUsageService _emailUsageService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        public AppointmentService(IAppointmentWriteRepository appointmentWriteRepository,
            IAppointmentReadRepository appointmentReadRepository, IBusinessRepository businessRepository,
            IEmailService emailService, SmsTextMagicService smsTextMagicService, IServicesRepository servicesRepository,
            IStaffRepository staffRepository, IEmailUsageService emailUsageService, IConfiguration configuration, INotificationService notificationService)
        {
            _appointmentWriteRepository = appointmentWriteRepository;
            _appointmentReadRepository = appointmentReadRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;
            _smsTextMagicService = smsTextMagicService;
            _servicesRepository = servicesRepository;
            _staffRepository = staffRepository;
            _emailUsageService = emailUsageService;
            _configuration = configuration;
            _notificationService = notificationService;
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

            // Convert appointment time to Vienna local time
            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointmentDto.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            var subject = "Appointment Received - Termin erhalten";

            // Add the appointment to the database
            try
            {
                // Add appointment and notification in parallel
                var addAppointmentTask = _appointmentWriteRepository.AddAppointmentAsync(appointment);
                var addNotificationTask = _notificationService.AddNotificationAsync(
                    appointmentDto.BusinessId,
                    $"Khách {customer.Name} đã đặt 1 lịch lúc {appointmentTimeFormatted}.",
                    NotificationType.Add
                );

                // Run both tasks in parallel
                await Task.WhenAll(addAppointmentTask, addNotificationTask);

                // Send email to the customer if email is provided
                if (!string.IsNullOrWhiteSpace(customer.Email))
                {
                    var bodySms = 
                        $"Hi, \n\nThank you for choosing {business.Name}. We have received your appointment request for {appointmentTimeFormatted}. We will confirm the details with you shortly. If needed, we’ll contact you via email or phone. \n\nBest regards,\n{business.Name}" +
                        $"\n\n---\n\n" +
                        $"Hallo, \n\nVielen Dank, dass Sie {business.Name} gewählt haben. Wir haben Ihre Terminanfrage für {appointmentTimeFormatted} erhalten. Wir werden die Details in Kürze bestätigen. Bei Bedarf werden wir Sie per E-Mail oder Telefon kontaktieren. \n\nLiebe Grüße,\n{business.Name}";

                    var emailMessage = new EmailMessage
                    {
                        ToEmail = customer.Email,
                        Subject = subject,
                        Body = bodySms
                    };

                    // Send email directly using _emailService
                    await _emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Body);
                }
                else
                {
                    // Log a message that no email was provided, but no error is returned
                    Console.WriteLine("No email provided for the customer, appointment still created successfully.");
                }

                // Send notification email to the business
                var businessNotificationSubject = "Yêu cầu đặt hẹn mới";
                var businessNotificationBody =
                    $"Khách hàng {customer.Name} đã yêu cầu một cuộc hẹn vào {appointmentTimeFormatted}. Vui lòng xem lại chi tiết cuộc hẹn và xác nhận theo yêu cầu.";

                if (!string.IsNullOrWhiteSpace(business.Email))
                {
                    var businessEmailMessage = new EmailMessage
                    {
                        ToEmail = business.Email,
                        Subject = businessNotificationSubject,
                        Body = businessNotificationBody
                    };

                    // Send email directly using _emailService
                    await _emailService.SendEmailAsync(businessEmailMessage.ToEmail, businessEmailMessage.Subject,
                        businessEmailMessage.Body);
                }
                else
                {
                    // Log message for business email failure, but don't return an error
                    Console.WriteLine("Failed to send notification email to business. Appointment was still created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save appointment: {ex.Message}");
            }
            
            var subjectReminder = "Appointment Reminder - Termin Erinnerung";

            // Schedule the reminder email 48 hours (2 days) before the appointment if email is provided
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                var bodyEmail =
                    $"Dear Customer, \n\nThis is a friendly reminder of your upcoming appointment at {business.Name} scheduled for {appointmentTimeFormatted}. Please ensure you arrive on time. We look forward to seeing you! \n\nBest regards,\n{business.Name}" +
                    $"\n\n---\n\n" +
                    $"Hallo, \n\nDies ist eine freundliche Erinnerung an Ihren bevorstehenden Termin bei {business.Name}, der für {appointmentTimeFormatted} geplant ist. Bitte stellen Sie sicher, dass Sie pünktlich erscheinen. Wir freuen uns darauf, Sie zu sehen! \n\nLiebe Grüße,\n{business.Name}";


                var sendTime = viennaTime.AddDays(-2);

                if (sendTime <= DateTime.UtcNow)
                {
                    // If the send time is in the past (less than 48 hours left), send the email immediately
                    var reminderEmailSent = await _emailService.SendEmailAsync(customer.Email, subjectReminder, bodyEmail);
                    if (reminderEmailSent)
                    {
                        // Update EmailUsage for reminder email (optional)
                    }
                }
                else
                {
                    // Schedule a reminder email via Hangfire
                    BackgroundJob.Schedule(
                        () => SendReminderEmail(customer.Email, subjectReminder, bodyEmail, appointmentDto.BusinessId),
                        new DateTimeOffset(sendTime));
                }
            }
            else
            {
                // Log message if reminder email can't be scheduled
                Console.WriteLine("No email provided for the customer, no reminder email scheduled.");
            }

            // Return true regardless of email failures, as long as the appointment is successfully created
            return true;
        }


        // Separate method for sending the reminder email and updating the email usage
        public async Task SendReminderEmail(string email, string subject, string bodyEmail, int businessId)
        {
            var emailMessage = new EmailMessage
            {
                ToEmail = email,
                Subject = subject,
                Body = bodyEmail
            };

            try
            {
                // Send the email directly using _emailService
                await _emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Body);

                // Optionally update the email usage stats after sending the email
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending the reminder email: {ex.Message}");
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

            // Convert appointment time to Vienna local time
            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            var subject = "Appointment Confirmation - Terminbestätigung";
            var bodySms =
                $"Dear Customer, \n\nThank you for choosing {business.Name}. We are pleased to confirm your appointment at {appointmentTimeFormatted}. We look forward to serving you! \n\nBest regards,\n{business.Name}" +
                $"\n\n---\n\n" +
                $"Hallo, \n\nVielen Dank, dass Sie {business.Name} gewählt haben. Wir freuen uns, Ihren Termin für {appointmentTimeFormatted} bestätigen zu können. Wir freuen uns darauf, Ihnen zu dienen! \n\nLiebe Grüße,\n{business.Name}";


            var emailMessage = new EmailMessage
            {
                ToEmail = customer.Email ?? string.Empty,
                Subject = subject,
                Body = bodySms
            };

            try
            {
                // Send email directly using _emailService
                if (!string.IsNullOrWhiteSpace(emailMessage.ToEmail))
                {
                    await _emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Body);
                }
                else
                {
                    Console.WriteLine("No customer email provided, skipping email notification.");
                }

                // Optionally update EmailUsage (if needed)
                // await _emailUsageService.AddEmailUsageAsync(new EmailUsage
                // {
                //     BusinessId = appointment.BusinessId,
                //     Year = DateTime.UtcNow.Year,
                //     Month = DateTime.UtcNow.Month,
                //     EmailCount = 1
                // });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending the email: {ex.Message}");
            }

            // Update the appointment status in the database
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