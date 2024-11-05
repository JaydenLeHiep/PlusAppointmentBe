using Hangfire;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.CheckInRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.NotificationRepo;
using PlusAppointment.Services.Interfaces.CheckInService;
using Microsoft.Extensions.Options;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;
using PlusAppointment.Repositories.Interfaces.DiscountTierRepo;
using PlusAppointment.Services.Interfaces.EmailSendingService;

namespace PlusAppointment.Services.Implementations.CheckInService;

public class CheckInService : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IBusinessRepository _businessRepository;
    private readonly IDiscountTierRepository _discountTierRepository;
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public CheckInService(ICheckInRepository checkInRepository, ICustomerRepository customerRepository,
        INotificationRepository notificationRepository, IEmailService emailService, IOptions<AppSettings> appSettings,
        IBusinessRepository businessRepository, IDiscountTierRepository discountTierRepository, IDiscountCodeRepository discountCodeRepository)
    {
        _checkInRepository = checkInRepository;
        _customerRepository = customerRepository;
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _appSettings = appSettings;
        _businessRepository = businessRepository;
        _discountTierRepository = discountTierRepository;
        _discountCodeRepository = discountCodeRepository;
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
        var hasCheckedInToday = await _checkInRepository.HasCheckedInTodayAsync(
            checkIn.BusinessId, checkIn.CustomerId, checkIn.CheckInTime);
    
        // if (hasCheckedInToday)
        // {
        //     throw new InvalidOperationException("Customer has already checked in today.");
        // }
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
            CreatedAt = DateTime.UtcNow
        });

        // Await both tasks to complete
        await Task.WhenAll(checkInTask, notificationTask);

        var frontendBaseUrl = _appSettings.Value.FrontendBaseUrl;
        var bookingAppointmentLink = $"{frontendBaseUrl}/customer-dashboard?business_name={business.Name}";

        // Step 2: Send the email immediately after check-in
        var emailSubject = "Thanks for checking in!";
        var emailBody = GenerateImmediateCheckInEmailContent(customer, business.Name, bookingAppointmentLink);

        // Send email directly
        await _emailService.SendEmailAsync(customer.Email, emailSubject, emailBody);

        // Step 3: Schedule follow-up email
        var followUpTime = checkIn.CheckInTime.AddDays(14);
        if (followUpTime > DateTime.UtcNow)
        {
            BackgroundJob.Schedule(() =>
                    _emailService.SendEmailAsync(customer.Email,
                        $"How was your visit at {business.Name}? Book your next appointment!",
                        GenerateFollowUpEmailContent(customer, business.Name)),
                new DateTimeOffset(followUpTime));
        }

        // Step 4: Calculate cumulative check-ins and apply discount if applicable
        var cumulativeCheckInCount = (await _checkInRepository.GetCheckInsByBusinessIdAsync(checkIn.BusinessId))
            .Count(c => c.CustomerId == checkIn.CustomerId);

        var discountTiers = await _discountTierRepository.GetDiscountTiersByBusinessIdAsync(checkIn.BusinessId);
        var applicableDiscount = discountTiers
            .Where(tier => cumulativeCheckInCount % tier.CheckInThreshold == 0) // Apply discount at threshold multiples
            .OrderByDescending(tier => tier.CheckInThreshold)
            .FirstOrDefault();

        if (applicableDiscount != null)
        {
            // Step 7: Generate a unique discount code
            var discountCode = new DiscountCode
            {
                Code = GenerateUniqueCode(),
                DiscountPercentage = applicableDiscount.DiscountPercentage,
                IsUsed = false,
                GeneratedAt = DateTime.UtcNow
            };
            await _discountCodeRepository.AddDiscountCodeAsync(discountCode);
            
            
            var discountEmailSubject = "Sie haben einen Rabatt verdient!";

            var discountEmailBody =
                GenerateDiscountEmailContentWithCode(customer, business.Name, applicableDiscount.DiscountPercentage, discountCode.Code);

            await _emailService.SendEmailAsync(customer.Email, discountEmailSubject, discountEmailBody);

            
        }
    }
    private string GenerateUniqueCode()
    {
        // Generate a simple unique code (e.g., based on GUID, you can customize the format)
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    private string GenerateDiscountEmailContentWithCode(Customer customer, string businessName, decimal discountPercentage, string discountCode)
    {
        return $@"
        <p>Hallo {customer.Name},</p>
        <p>Vielen Dank für Ihre Treue! Sie haben einen Rabatt von {discountPercentage}% bei Ihrem nächsten Besuch bei {businessName} verdient.</p>
        <p>Ihr Rabattcode: <b>{discountCode}</b></p>
        <p>Wir freuen uns darauf, Sie bald wiederzusehen!</p>
    ";
    }



    private string GenerateImmediateCheckInEmailContent(Customer customer, string businessName,
        string bookingAppointmentLink)
    {
        return $@"
        <p>Hallo {customer.Name},</p>
        <p>Vielen Dank, dass Sie bei {businessName} eingecheckt haben. Nächstes Mal können Sie online bei uns buchen: <a href='{bookingAppointmentLink}'>Jetzt buchen</a></p>
        <p>Vielen Dank!</p>
    ";
    }


    private string GenerateFollowUpEmailContent(Customer customer, string businessName)
    {
        // Access the frontend base URL from appsettings
        var frontendBaseUrl = _appSettings.Value.FrontendBaseUrl;
        var bookingLink = $"{frontendBaseUrl}/customer-dashboard?business_name={Uri.EscapeDataString(businessName)}";

        // Shorter email content in both English and German to encourage a new booking
        return $@"
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