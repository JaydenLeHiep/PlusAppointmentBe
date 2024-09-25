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
using PlusAppointment.Services.Interfaces.NotificationService;
using PlusAppointment.Utils.SendingEmail;



namespace PlusAppointment.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        //private readonly IAppointmentRepository _appointmentRepository;

        private readonly IAppointmentWriteRepository _appointmentWriteRepository;
        private readonly IAppointmentReadRepository _appointmentReadRepository;
        private readonly IBusinessRepository _businessRepository;

        private readonly IEmailService _emailService;

        private readonly IServicesRepository _servicesRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IEmailUsageService _emailUsageService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        public AppointmentService(IAppointmentWriteRepository appointmentWriteRepository,
            IAppointmentReadRepository appointmentReadRepository, IBusinessRepository businessRepository,
            IEmailService emailService, IServicesRepository servicesRepository,
            IStaffRepository staffRepository, IEmailUsageService emailUsageService, IConfiguration configuration,
            INotificationService notificationService)
        {
            _appointmentWriteRepository = appointmentWriteRepository;
            _appointmentReadRepository = appointmentReadRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;

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
            // Fetch the business and customer details concurrently
            var businessTask = _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            var customerTask = _appointmentReadRepository.GetByCustomerIdAsync(appointmentDto.CustomerId);

            await Task.WhenAll(businessTask, customerTask);

            var business = await businessTask;
            var customer = await customerTask;

            if (business == null) throw new ArgumentException("Invalid BusinessId");
            if (customer == null) throw new ArgumentException("Invalid CustomerId");

            // Prepare service and staff validation tasks
            var serviceStaffValidationTasks = appointmentDto.Services.Select(async serviceStaffDto =>
            {
                var serviceTask = _servicesRepository.GetByIdAsync(serviceStaffDto.ServiceId);
                var staffTask = _staffRepository.GetByIdAsync(serviceStaffDto.StaffId);

                await Task.WhenAll(serviceTask, staffTask);

                var service = await serviceTask;
                var staff = await staffTask;

                if (service == null || staff == null)
                {
                    throw new ArgumentException("Invalid ServiceId or StaffId");
                }

                return new AppointmentServiceStaffMapping
                {
                    ServiceId = service.ServiceId,
                    StaffId = staff.StaffId,
                    Service = service,
                    Staff = staff
                };
            }).ToList();

            // Await all service/staff validation tasks
            var mappings = await Task.WhenAll(serviceStaffValidationTasks);

            // Create the appointment
            var appointment = new Appointment
            {
                CustomerId = appointmentDto.CustomerId,
                Customer = customer,
                BusinessId = appointmentDto.BusinessId,
                Business = business,
                AppointmentTime = appointmentDto.AppointmentTime,
                Duration = TimeSpan.Zero, // Duration should be calculated if needed
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comment = appointmentDto.Comment,
                AppointmentServices = mappings.ToList()
            };

            // Add the appointment to the database and notify in parallel
            var addAppointmentTask = _appointmentWriteRepository.AddAppointmentAsync(appointment);
            var appointmentTimeFormatted = TimeZoneInfo.ConvertTimeFromUtc(appointmentDto.AppointmentTime,
                    TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                .ToString("HH:mm 'on' dd.MM.yyyy");

            var addNotificationTask = _notificationService.AddNotificationAsync(
                appointmentDto.BusinessId,
                $"Customer {customer.Name} booked an appointment for {appointmentTimeFormatted}.",
                NotificationType.Add
            );

            await Task.WhenAll(addAppointmentTask, addNotificationTask);

            // Email and notification sending tasks (non-blocking)
            var emailTasks = new List<Task>();

            // Send customer confirmation email
            // Send customer confirmation email
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                var customerEmailBody =
                    $"<p>Hi {customer.Name},</p>" +
                    $"<p>Your appointment at <strong>{business.Name}</strong> for <strong>{appointmentTimeFormatted}</strong> is confirmed.</p>" +
                    $"<hr>" +
                    $"<p>Hallo {customer.Name},</p>" +
                    $"<p>Ihr Termin bei <strong>{business.Name}</strong> für <strong>{appointmentTimeFormatted}</strong> ist bestätigt.</p>";

                emailTasks.Add(_emailService.SendEmailAsync(customer.Email, "Appointment Confirmation",
                    customerEmailBody));
            }

            // Send notification email to the business
            if (!string.IsNullOrWhiteSpace(business.Email))
            {
                var businessEmailBody =
                    $"<p>Khách hàng {customer.Name} đã đặt lịch hẹn vào lúc <strong>{appointmentTimeFormatted}</strong>. Vui lòng kiểm tra và xác nhận chi tiết.</p>";

                emailTasks.Add(_emailService.SendEmailAsync(business.Email, "Yêu cầu đặt lịch hẹn mới", businessEmailBody));
            }


            // Schedule a reminder email if necessary
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                // Reminder email body in English and German
                var reminderBody =
                    $"<p>Hi {customer.Name},</p>" +
                    $"<p>Reminder: Your appointment at <strong>{business.Name}</strong> is on <strong>{appointmentTimeFormatted}</strong>.</p>" +
                    $"<hr>" +
                    $"<p>Hallo {customer.Name},</p>" +
                    $"<p>Erinnerung: Ihr Termin bei <strong>{business.Name}</strong> ist am <strong>{appointmentTimeFormatted}</strong>.</p>";

                // Calculate the reminder times
                var reminderTime24Hours = appointmentDto.AppointmentTime.AddHours(-24);  // 24-hour reminder
                var reminderTime2Hours = appointmentDto.AppointmentTime.AddHours(-2);    // 2-hour reminder

                // Send the 24-hour reminder
                if (reminderTime24Hours > DateTime.UtcNow)
                {
                    BackgroundJob.Schedule(() =>
                            _emailService.SendEmailAsync(customer.Email, "Appointment Reminder / Termin-Erinnerung", reminderBody),
                        new DateTimeOffset(reminderTime24Hours));
                }
                else
                {
                    emailTasks.Add(_emailService.SendEmailAsync(customer.Email, "Appointment Reminder / Termin-Erinnerung", reminderBody));
                }

                // Send the 2-hour reminder
                if (reminderTime2Hours > DateTime.UtcNow)
                {
                    BackgroundJob.Schedule(() =>
                            _emailService.SendEmailAsync(customer.Email, "Appointment Reminder / Termin-Erinnerung", reminderBody),
                        new DateTimeOffset(reminderTime2Hours));
                }
                else
                {
                    emailTasks.Add(_emailService.SendEmailAsync(customer.Email, "Appointment Reminder / Termin-Erinnerung", reminderBody));
                }
            }



            // Run all email tasks in parallel
            await Task.WhenAll(emailTasks);

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
            // Update the appointment with the new details
            await _appointmentWriteRepository.UpdateAppointmentWithServicesAsync(id, updateAppointmentDto);

            // Fetch the appointment, customer, and business details concurrently
            var appointmentTask = _appointmentReadRepository.GetAppointmentByIdAsync(id);

            // Once appointmentTask completes, use the results to fetch customer and business details
            var appointment = await appointmentTask;
            if (appointment == null) throw new KeyNotFoundException("Appointment not found");

            var customerTask = _appointmentReadRepository.GetByCustomerIdAsync(appointment.CustomerId);
            var businessTask = _businessRepository.GetByIdAsync(appointment.BusinessId);

            await Task.WhenAll(customerTask, businessTask);

            var customer = await customerTask;
            var business = await businessTask;

            if (customer == null) throw new ArgumentException("Invalid CustomerId");
            if (business == null) throw new ArgumentException("Invalid BusinessId");

            // Convert the appointment time to Vienna local time
            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            // Prepare the email in both English and German
            var subject = "Appointment Updated - Terminänderung";
            var bodyEmail =
                $"<p>Hi {customer.Name},</p>" +
                $"<p>Your appointment at <strong>{business.Name}</strong> has been updated. The new time is <strong>{appointmentTimeFormatted}</strong>.</p>" +
                $"<p>If you have any questions, reach us at <strong>{business.Phone}</strong>. See you soon!</p>" +
                $"<hr>" +
                $"<p>Hallo {customer.Name},</p>" +
                $"<p>Ihr Termin bei <strong>{business.Name}</strong> wurde geändert. Die neue Uhrzeit ist <strong>{appointmentTimeFormatted}</strong>.</p>" +
                $"<p>Falls Sie Fragen haben, erreichen Sie uns unter <strong>{business.Phone}</strong>. Wir freuen uns auf Ihren Besuch!</p>";


            var emailMessage = new EmailMessage
            {
                ToEmail = customer.Email ?? string.Empty,
                Subject = subject,
                Body = bodyEmail
            };

            try
            {
                // Send email asynchronously
                if (!string.IsNullOrWhiteSpace(emailMessage.ToEmail))
                {
                    await _emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Body);
                }
                else
                {
                    Console.WriteLine("No customer email provided, skipping email notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending the email: {ex.Message}");
            }
        }


        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            // Fetch appointment, customer, and business in parallel
            var appointmentTask = _appointmentReadRepository.GetAppointmentByIdAsync(id);
            var appointment = await appointmentTask;

            if (appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found");
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Fetch customer and business details concurrently
            var customerTask = _appointmentReadRepository.GetByCustomerIdAsync(appointment.CustomerId);
            var businessTask = _businessRepository.GetByIdAsync(appointment.BusinessId);

            await Task.WhenAll(customerTask, businessTask);

            var customer = customerTask.Result;
            var business = businessTask.Result;

            if (customer == null)
            {
                throw new ArgumentException("Invalid CustomerId");
            }

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
                $"Dear Customer, your appointment at {business.Name} is confirmed for {appointmentTimeFormatted}. We look forward to serving you!" +
                $"<hr>" +
                $"Hallo, Ihr Termin bei {business.Name} ist für {appointmentTimeFormatted} bestätigt. Wir freuen uns auf Sie!";


            var emailMessage = new EmailMessage
            {
                ToEmail = customer.Email ?? string.Empty,
                Subject = subject,
                Body = bodySms
            };

            var emailSendingTask = Task.CompletedTask;

            try
            {
                // Send email directly using _emailService in parallel if the customer email is provided
                if (!string.IsNullOrWhiteSpace(emailMessage.ToEmail))
                {
                    emailSendingTask =
                        _emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Body);
                }
                else
                {
                    Console.WriteLine("No customer email provided, skipping email notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending the email: {ex.Message}");
            }

            // Update the appointment status in the database in parallel with email sending
            var updateStatusTask = _appointmentWriteRepository.UpdateAppointmentStatusAsync(appointment);

            // Wait for both tasks to complete
            await Task.WhenAll(emailSendingTask, updateStatusTask);
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