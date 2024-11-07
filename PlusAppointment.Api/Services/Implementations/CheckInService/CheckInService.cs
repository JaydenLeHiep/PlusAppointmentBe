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
        IBusinessRepository businessRepository, IDiscountTierRepository discountTierRepository,
        IDiscountCodeRepository discountCodeRepository)
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

    private (DiscountTier? applicableDiscount, int? nextDiscountCheckIns, DiscountTier? nextDiscountTier)
        CalculateDiscount(int cumulativeCheckInCount, List<DiscountTier> discountTiers)
    {
        // Sort discount tiers from highest to lowest threshold for evaluation
        var sortedDiscountTiers = discountTiers.OrderByDescending(t => t.CheckInThreshold).ToList();

        int? minCheckInsUntilNextDiscount = null;
        DiscountTier? upcomingDiscountTier = null;

        // Check each discount tier from highest to lowest to find applicable discount
        foreach (var tier in sortedDiscountTiers)
        {
            if ((cumulativeCheckInCount + 1) % tier.CheckInThreshold == 0)
            {
                // If the current check-in count plus 1 divides evenly by the tier's threshold, return this tier as applicable
                return (tier, null, null);
            }
        }

        // Find the next nearest check-in count for any tier if no applicable discount is found
        foreach (var tier in sortedDiscountTiers)
        {
            int checkInsUntilNext = tier.CheckInThreshold - ((cumulativeCheckInCount + 1) % tier.CheckInThreshold);
            if (checkInsUntilNext > 0 && (!minCheckInsUntilNextDiscount.HasValue ||
                                          checkInsUntilNext < minCheckInsUntilNextDiscount))
            {
                minCheckInsUntilNextDiscount = checkInsUntilNext;
                upcomingDiscountTier = tier;
            }
        }

        // Return the number of check-ins until the next discount and the next discount tier, if applicable
        return (null, minCheckInsUntilNextDiscount, upcomingDiscountTier);
    }


    public async Task AddCheckInAsync(CheckIn? checkIn)
    {
        if (checkIn == null)
        {
            throw new ArgumentNullException(nameof(checkIn), "CheckIn cannot be null.");
        }
        // Step 1: Check if the customer has checked in today
        var hasCheckedInToday = await _checkInRepository.HasCheckedInTodayAsync(
            checkIn.BusinessId, checkIn.CustomerId, checkIn.CheckInTime);
        
        if (hasCheckedInToday)
        {
            throw new InvalidOperationException("Customer has already checked in today.");
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

        // Step 2: Retrieve the discount tiers for the business
        var discountTiers = await _discountTierRepository.GetDiscountTiersByBusinessIdAsync(checkIn.BusinessId);

        var enumerable = discountTiers as DiscountTier[] ?? discountTiers.ToArray();
        if (!enumerable.Any())
        {
            // Business does not participate in the discount campaign; send a basic thank-you email
            var emailSubject = "Danke für Ihren Besuch!";
            var frontendBaseUrl = _appSettings.Value.FrontendBaseUrl;
            var bookingAppointmentLink = $"{frontendBaseUrl}/customer-dashboard?business_name={business.Name}";

            var emailBody =
                GenerateImmediateCheckInEmailContent(customer, business.Name, bookingAppointmentLink, string.Empty);
            await _emailService.SendEmailAsync(customer.Email, emailSubject, emailBody);

            // Save the check-in without calculating discounts
            await _checkInRepository.AddCheckInAsync(checkIn);
            return;
        }

        // Step 3: Get all previous check-ins for the customer at the business
        var previousCheckIns = await _checkInRepository.GetCheckInsByBusinessIdAsync(checkIn.BusinessId);
        var customerCheckIns = previousCheckIns.Where(c => c.CustomerId == checkIn.CustomerId).ToList();
        var cumulativeCheckInCount = customerCheckIns.Count;

        // Step 4: Calculate if the current check-in is eligible for a discount
        var (applicableDiscount, nextDiscountCheckIns, nextDiscountTier) =
            CalculateDiscount(cumulativeCheckInCount, enumerable.ToList());

        // Step 5: Save the check-in and send a notification
        var checkInTask = _checkInRepository.AddCheckInAsync(checkIn);
        var notificationTask = _notificationRepository.AddNotificationAsync(new Notification
        {
            BusinessId = checkIn.BusinessId,
            Message = $"Khách hàng {customer.Name} đã đến. Vui lòng lưu ý.", // Vietnamese text
            NotificationType = NotificationType.CheckIn,
            CreatedAt = DateTime.UtcNow
        });
        await Task.WhenAll(checkInTask, notificationTask);

        // Step 6: Prepare and send the immediate check-in email for discount campaign participants
        var frontendBaseUrlForDiscount = _appSettings.Value.FrontendBaseUrl;
        var bookingAppointmentLinkForDiscount =
            $"{frontendBaseUrlForDiscount}/customer-dashboard?business_name={business.Name}";

        string additionalInfo = nextDiscountCheckIns.HasValue && nextDiscountTier != null
            ? $"<p>Sie haben noch {nextDiscountCheckIns.Value} mal einzuchecken, um {nextDiscountTier.DiscountPercentage}% Rabatt zu erhalten.</p>"
            : string.Empty;

        var discountEmailSubject = "Danke für Ihren Besuch!";
        var discountEmailBody = GenerateImmediateCheckInEmailContent(customer, business.Name,
            bookingAppointmentLinkForDiscount, additionalInfo);
        await _emailService.SendEmailAsync(customer.Email, discountEmailSubject, discountEmailBody);

        // Step 7: Schedule a follow-up email
        var followUpTime = checkIn.CheckInTime.AddDays(14);
        if (followUpTime > DateTime.UtcNow)
        {
            BackgroundJob.Schedule(() =>
                    _emailService.SendEmailAsync(customer.Email,
                        $"Wie war Ihr Besuch bei {business.Name}? Buchen Sie Ihren nächsten Termin!",
                        GenerateFollowUpEmailContent(customer, business.Name)),
                new DateTimeOffset(followUpTime));
        }

        // Step 8: Generate and send a discount code if applicable
        if (applicableDiscount != null)
        {
            var discountCode = new DiscountCode
            {
                Code = GenerateUniqueCode(),
                DiscountPercentage = applicableDiscount.DiscountPercentage,
                IsUsed = false,
                GeneratedAt = DateTime.UtcNow
            };
            await _discountCodeRepository.AddDiscountCodeAsync(discountCode);

            var discountCodeEmailSubject = "Sie haben einen Rabatt verdient!";
            var discountCodeEmailBody = GenerateDiscountEmailContentWithCode(customer, business.Name,
                applicableDiscount.DiscountPercentage, discountCode.Code);
            await _emailService.SendEmailAsync(customer.Email, discountCodeEmailSubject, discountCodeEmailBody);
        }
    }


    private string GenerateImmediateCheckInEmailContent(Customer customer, string businessName,
        string bookingAppointmentLink, string additionalInfo)
    {
        return $@"
    <p>Hallo {customer.Name},</p>
    <p>Vielen Dank, dass Sie bei {businessName} eingecheckt haben. Nächstes Mal können Sie online bei uns buchen: <a href='{bookingAppointmentLink}'>Jetzt buchen</a></p>
    {additionalInfo}
    <p>Vielen Dank!</p>
    ";
    }

    private string GenerateUniqueCode()
    {
        // Generate a simple unique code (e.g., based on GUID, you can customize the format)
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    private string GenerateDiscountEmailContentWithCode(Customer customer, string businessName,
        decimal discountPercentage, string discountCode)
    {
        return $@"
        <p>Hallo {customer.Name},</p>
        <p>Vielen Dank für Ihre Treue! Sie haben einen Rabatt von {discountPercentage}% bei Ihrem nächsten Besuch bei {businessName} verdient.</p>
        <p>Ihr Rabattcode: <b>{discountCode}</b></p>
        <p>Wir freuen uns darauf, Sie bald wiederzusehen!</p>
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