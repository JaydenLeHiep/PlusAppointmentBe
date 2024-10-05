using Hangfire;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Services.Interfaces.CheckInService;
using PlusAppointment.Utils.SendingEmail;
using Microsoft.Extensions.Options;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;

namespace PlusAppointment.Services.Implementations.CheckInService;

public class CheckInService : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IBusinessRepository _businessRepository;

    public CheckInService(ICheckInRepository checkInRepository, ICustomerRepository customerRepository,
        INotificationRepository notificationRepository, IEmailService emailService, IOptions<AppSettings> appSettings,
        IBusinessRepository businessRepository)
    {
        _checkInRepository = checkInRepository;
        _customerRepository = customerRepository;
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _appSettings = appSettings;
        _businessRepository = businessRepository;
    }

    public async Task<IEnumerable<CheckIn?>> GetAllCheckInsAsync()
    {
        return await _checkInRepository.GetAllCheckInsAsync();
    }

    public async Task<CheckIn?> GetCheckInByIdAsync(int id)
    {
        var checkIn = await _checkInRepository.GetCheckInByIdAsync(id);
        if (checkIn == null)
        {
            return null;
        }

        return checkIn;
    }

    public async Task<IEnumerable<CheckIn?>> GetCheckInsByBusinessIdAsync(int businessId)
    {
        return await _checkInRepository.GetCheckInsByBusinessIdAsync(businessId);
    }

    public async Task AddCheckInAsync(CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn), "CheckIn cannot be null.");
        }

        // Step 1: Fetch customer and business details
        var customerTask = _customerRepository.GetCustomerByIdAsync(checkIn.CustomerId);
        var businessTask = _businessRepository.GetByIdAsync(checkIn.BusinessId);

        await Task.WhenAll(customerTask, businessTask);

        var customer = await customerTask;
        var business = await businessTask;

        if (customer == null || business == null)
        {
            throw new KeyNotFoundException("Customer or Business not found.");
        }

        // Prepare tasks for adding check-in and notification
        var checkInTask = _checkInRepository.AddCheckInAsync(checkIn);
        var notificationTask = _notificationRepository.AddNotificationAsync(new Notification
        {
            BusinessId = checkIn.BusinessId,
            Message = $"Khách hàng {customer.Name} đã đến. Vui lòng lưu ý.", // Vietnamese text
            NotificationType = NotificationType.CheckIn,
            CreatedAt = DateTime.UtcNow // Assuming you have this property
        });

        // Await both tasks to complete
        await Task.WhenAll(checkInTask, notificationTask);
        // Step 3: Schedule follow-up email
        var followUpTime = checkIn.CheckInTime.AddDays(14);
        if (followUpTime > DateTime.UtcNow)
        {
            BackgroundJob.Schedule(() =>
                    _emailService.SendEmailAsync(customer.Email, $"How was your visit at {business.Name}? Book your next appointment!",
                        GenerateFollowUpEmailContent(customer, business.Name)),
                new DateTimeOffset(followUpTime));
        }
    }

    private string GenerateFollowUpEmailContent(Customer customer, string businessName)
    {
        // Access the frontend base URL from appsettings
        var frontendBaseUrl = _appSettings.Value.FrontendBaseUrl;
        var bookingLink = $"{frontendBaseUrl}/customer-dashboard?business_name={Uri.EscapeDataString(businessName)}";

        // Shorter email content in both English and German to encourage a new booking
        return $@"
        <p>Dear {customer.Name},</p>
        <p>We hope you enjoyed your recent visit at {businessName}. Book your next appointment now: <a href='{bookingLink}'>Book Now</a></p>
        <p>Thank you!</p>
        <hr>
        <p>Hallo {customer.Name},</p>
        <p>Wir hoffen, dass Ihnen Ihr letzter Besuch bei {businessName} gefallen hat. Buchen Sie jetzt Ihren nächsten Termin: <a href='{bookingLink}'>Jetzt buchen</a></p>
        <p>Vielen Dank!</p>
    ";
    }



    public async Task UpdateCheckInAsync(int checkInId, CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn), "CheckIn cannot be null.");
        }

        var existingCheckIn = await _checkInRepository.GetCheckInByIdAsync(checkInId);
        if (existingCheckIn == null)
        {
            throw new KeyNotFoundException("CheckIn not found.");
        }

        // Update fields if provided
        existingCheckIn.CustomerId = checkIn.CustomerId;
        existingCheckIn.BusinessId = checkIn.BusinessId;
        existingCheckIn.CheckInTime = checkIn.CheckInTime;
        existingCheckIn.CheckInType = checkIn.CheckInType;

        await _checkInRepository.UpdateCheckInAsync(existingCheckIn);
    }

    public async Task DeleteCheckInAsync(int checkInId)
    {
        await _checkInRepository.DeleteCheckInAsync(checkInId);
    }
}