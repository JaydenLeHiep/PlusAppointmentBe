using Hangfire;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentRead;
using PlusAppointment.Repositories.Interfaces.AppointmentRepo.AppointmentWrite;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.ServicesRepo;
using PlusAppointment.Repositories.Interfaces.StaffRepo;
using PlusAppointment.Services.Interfaces.AppointmentService;
using PlusAppointment.Services.Interfaces.EmailUsageService;
using Microsoft.Extensions.Options;
using PlusAppointment.Models.DTOs.Appointment;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Services.Interfaces.EmailSendingService;

namespace PlusAppointment.Services.Implementations.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentWriteRepository _appointmentWriteRepository;
        private readonly IAppointmentReadRepository _appointmentReadRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IEmailService _emailService;
        private readonly IServicesRepository _servicesRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IEmailUsageService _emailUsageService;
        private readonly INotificationRepository _notificationRepo;
        private readonly IOptions<AppSettings> _appSettings;

        public AppointmentService(IAppointmentWriteRepository appointmentWriteRepository,
            IAppointmentReadRepository appointmentReadRepository, IBusinessRepository businessRepository,
            IEmailService emailService, IServicesRepository servicesRepository,
            IStaffRepository staffRepository, IEmailUsageService emailUsageService,
            INotificationRepository notificationRepo, IOptions<AppSettings> appSettings)
        {
            _appointmentWriteRepository = appointmentWriteRepository;
            _appointmentReadRepository = appointmentReadRepository;
            _businessRepository = businessRepository;
            _emailService = emailService;
            _servicesRepository = servicesRepository;
            _staffRepository = staffRepository;
            _emailUsageService = emailUsageService;
            _notificationRepo = notificationRepo;
            _appSettings = appSettings;
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

        public async Task<(bool IsSuccess, AppointmentRetrieveDto? Appointment)> AddAppointmentAsync(
            AppointmentDto appointmentDto)
        {
            var businessTask = _businessRepository.GetByIdAsync(appointmentDto.BusinessId);
            var customerTask = _appointmentReadRepository.GetByCustomerIdAsync(appointmentDto.CustomerId);

            await Task.WhenAll(businessTask, customerTask);

            var business = await businessTask;
            var customer = await customerTask;

            if (business == null || customer == null)
            {
                return (false, null);
            }
            
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
            
            var mappings = await Task.WhenAll(serviceStaffValidationTasks);

            string status;
            if (business.RequiresAppointmentConfirmation)
            {
                status = "Pending";
            }
            else
            {
                status = "Confirm";
            }
            
            var appointment = new Appointment
            {
                CustomerId = appointmentDto.CustomerId,
                Customer = customer,
                BusinessId = appointmentDto.BusinessId,
                Business = business,
                AppointmentTime = appointmentDto.AppointmentTime,
                Duration = TimeSpan.Zero,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comment = appointmentDto.Comment,
                AppointmentServices = mappings.ToList()
            };
            
            var addAppointmentTask = _appointmentWriteRepository.AddAppointmentAsync(appointment);
            var appointmentTimeFormatted = TimeZoneInfo.ConvertTimeFromUtc(appointmentDto.AppointmentTime,
                    TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                .ToString("HH:mm 'on' dd.MM.yyyy");

            var addNotificationTask = _notificationRepo.AddNotificationAsync(new Notification
            {
                BusinessId = appointmentDto.BusinessId,
                Message = $"Khách hàng {customer.Name} đã đặt lịch hẹn vào lúc {appointmentTimeFormatted}.",
                NotificationType = NotificationType.Add,
                CreatedAt = DateTime.UtcNow
            });


            await Task.WhenAll(addAppointmentTask, addNotificationTask);
            
            var emailTasks = new List<Task>();
            var emailCount = 0;

            var frontendBaseUrl = _appSettings.Value.FrontendBaseUrl;
            var businessNameEncoded = Uri.EscapeDataString(business.Name);
            var addNewAppointmentLink = $"{frontendBaseUrl}/customer-dashboard?business_name={businessNameEncoded}";
            var cancelAppointmentLink = $"{frontendBaseUrl}/delete-appointment-customer?business_name={businessNameEncoded}";
            
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                var customerEmailBody =
                    $"<p>Hallo {customer.Name},</p>" +
                    $"<p>Ihr Termin bei <strong>{business.Name}</strong> für <strong>{appointmentTimeFormatted}</strong> ist bestätigt.</p>" +
                    $"<p>Falls Sie noch einen weiteren Termin buchen möchten, klicken Sie bitte <a href='{addNewAppointmentLink}'>hier</a>.</p>" +
                    $"<p>Wenn Sie Ihren Termin stornieren möchten, klicken Sie bitte <a href='{cancelAppointmentLink}'>hier</a>.</p>";
                emailTasks.Add(_emailService.SendEmailAsync(customer.Email, "Appointment Confirmation",
                    customerEmailBody));
                emailCount++;
            }

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                var reminderBody =
                    $"<p>Hallo {customer.Name},</p>" +
                    $"<p>Erinnerung: Ihr Termin bei <strong>{business.Name}</strong> ist am <strong>{appointmentTimeFormatted}</strong>.</p>" +
                    $"<p>Falls Sie noch einen weiteren Termin buchen möchten, klicken Sie bitte <a href='{addNewAppointmentLink}'>hier</a>.</p>";


                var reminderTime24Hours = appointmentDto.AppointmentTime.AddHours(-24);
                var reminderTime2Hours = appointmentDto.AppointmentTime.AddHours(-2);

                // Send the 24-hour reminder
                if (reminderTime24Hours > DateTime.UtcNow)
                {
                    BackgroundJob.Schedule(() =>
                            _emailService.SendEmailAsync(customer.Email, "Appointment Reminder / Termin-Erinnerung",
                                reminderBody),
                        new DateTimeOffset(reminderTime24Hours));
                    emailCount++;
                }
                else
                {
                    emailTasks.Add(_emailService.SendEmailAsync(customer.Email,
                        "Appointment Reminder / Termin-Erinnerung", reminderBody));
                    emailCount++;
                }

                // Send the 2-hour reminder
                if (reminderTime2Hours > DateTime.UtcNow)
                {
                    BackgroundJob.Schedule(() =>
                            _emailService.SendEmailAsync(customer.Email, "Appointment Reminder / Termin-Erinnerung",
                                reminderBody),
                        new DateTimeOffset(reminderTime2Hours));
                    emailCount++;
                }
                else
                {
                    emailTasks.Add(_emailService.SendEmailAsync(customer.Email,
                        "Appointment Reminder / Termin-Erinnerung", reminderBody));
                    emailCount++;
                }
            }
            
            if (!string.IsNullOrWhiteSpace(business.Email))
            {
                var businessEmailBody =
                    $"<p>Khách hàng {customer.Name} đã đặt lịch hẹn vào lúc <strong>{appointmentTimeFormatted}</strong>. Vui lòng kiểm tra chi tiết.</p>";

                emailTasks.Add(_emailService.SendEmailAsync(business.Email, "Yêu cầu đặt lịch hẹn mới",
                    businessEmailBody));
                emailCount++;
            }
            
            var emailUsageTask = _emailUsageService.AddEmailUsageAsync(new EmailUsage
            {
                BusinessId = appointmentDto.BusinessId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                EmailCount = emailCount
            });

            await Task.WhenAll(emailTasks);
            await emailUsageTask;
            
            var appointmentRetrieveDto = await MapToDtoAsync(appointment);
            
            return (true, appointmentRetrieveDto);
        }

        

        public async Task UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            await _appointmentWriteRepository.UpdateAppointmentWithServicesAsync(id, updateAppointmentDto);
            
            var appointmentTask = _appointmentReadRepository.GetAppointmentByIdAsync(id);

            var appointment = await appointmentTask;
            if (appointment == null) throw new KeyNotFoundException("Appointment not found");

            var customerTask = _appointmentReadRepository.GetByCustomerIdAsync(appointment.CustomerId);
            var businessTask = _businessRepository.GetByIdAsync(appointment.BusinessId);

            await Task.WhenAll(customerTask, businessTask);

            var customer = await customerTask;
            var business = await businessTask;

            if (customer == null) throw new ArgumentException("Invalid CustomerId");
            if (business == null) throw new ArgumentException("Invalid BusinessId");
            
            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");
            
            var subject = "Appointment Updated - Terminänderung";
            var bodyEmail =
                $"<p>Hallo {customer.Name},</p>" +
                $"<p>Ihr Termin bei <strong>{business.Name}</strong> wurde geändert. Die neue Uhrzeit ist <strong>{appointmentTimeFormatted}</strong>.</p>" +
                $"<p>Falls Sie Fragen haben, erreichen Sie uns unter <strong>{business.Phone}</strong>. Wir freuen uns auf Ihren Besuch!</p>";

            var emailMessage = new EmailMessage
            {
                ToEmail = customer.Email ?? string.Empty,
                Subject = subject,
                Body = bodyEmail
            };

            var emailTasks = new List<Task>();
            var emailCount = 0;

            try
            {
                if (!string.IsNullOrWhiteSpace(emailMessage.ToEmail))
                {
                    emailTasks.Add(_emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject,
                        emailMessage.Body));
                    emailCount++;
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
            
            var emailUsageTask = _emailUsageService.AddEmailUsageAsync(new EmailUsage
            {
                BusinessId = appointment.BusinessId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                EmailCount = emailCount
            });
            
            await Task.WhenAll(emailTasks.Append(emailUsageTask));
        }


        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            var appointmentTask = _appointmentReadRepository.GetAppointmentByIdAsync(id);
            var appointment = await appointmentTask;

            if (appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found");
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            
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
            
            TimeZoneInfo viennaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime viennaTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentTime, viennaTimeZone);
            var appointmentTimeFormatted = viennaTime.ToString("HH:mm 'on' dd.MM.yyyy");

            var subject = "Appointment Confirmation - Terminbestätigung";
            var bodySms =
                $"Hallo, Ihr Termin bei {business.Name} ist für {appointmentTimeFormatted} bestätigt. Wir freuen uns auf Sie!";

            var emailMessage = new EmailMessage
            {
                ToEmail = customer.Email ?? string.Empty,
                Subject = subject,
                Body = bodySms
            };

            var emailTasks = new List<Task>();
            var emailCount = 0; 

            try
            {
                if (!string.IsNullOrWhiteSpace(emailMessage.ToEmail))
                {
                    emailTasks.Add(_emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject,
                        emailMessage.Body));
                    emailCount++;
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
            
            var emailUsageTask = _emailUsageService.AddEmailUsageAsync(new EmailUsage
            {
                BusinessId = appointment.BusinessId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                EmailCount = emailCount 
            });
            
            var updateStatusTask = _appointmentWriteRepository.UpdateAppointmentStatusAsync(appointment);
            
            await Task.WhenAll(emailTasks.Append(emailUsageTask).Append(updateStatusTask));
        }


        public async Task DeleteAppointmentAsync(int id)
        {
            await _appointmentWriteRepository.DeleteAppointmentAsync(id);
        }

        public async Task DeleteAppointmentForCustomerAsync(int id)
        {
            var appointment = await _appointmentReadRepository.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                throw new ArgumentException("Invalid AppointmentId");
            }
            
            var customer = appointment.Customer;
            var businessId = appointment.BusinessId;
            var appointmentTime = appointment.AppointmentTime;
            
            var appointmentTimeFormatted = TimeZoneInfo.ConvertTimeFromUtc(appointmentTime,
                    TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                .ToString("HH:mm 'on' dd.MM.yyyy");
            
            await _appointmentWriteRepository.DeleteAppointmentForCustomerAsync(id);
            
            var notificationMessage =
                $"Khách hàng {customer.Name} đã hủy một cuộc hẹn vào lúc {appointmentTimeFormatted}.";

            await _notificationRepo.AddNotificationAsync(new Notification
            {
                BusinessId = businessId,
                Message = notificationMessage,
                NotificationType = NotificationType.Cancel,
                CreatedAt = DateTime.UtcNow
            });
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

        public async Task<List<AppointmentRetrieveDto>> GetAppointmentsByBusinessIdAsync(int businessId)
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

            foreach (var apptService in appointment.AppointmentServices)
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
                AppointmentTime = appointment.AppointmentTime,
                Duration = appointment.Duration,
                Status = appointment.Status,
                Comment = appointment.Comment ?? "No comment",
                Services = services
            };
        }
    }
}