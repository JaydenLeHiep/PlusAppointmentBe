using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Repositories.Interfaces.DiscountCodeRepo;
using PlusAppointment.Services.Interfaces.EmailSendingService;

namespace PlusAppointment.CronJobs.EmailJob
{
    public class BirthdayEmailJob
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IDiscountCodeRepository _discountCodeRepository;
        private readonly IEmailService _emailService;

        public BirthdayEmailJob(ICustomerRepository customerRepository, IEmailService emailService, 
                                IBusinessRepository businessRepository, IDiscountCodeRepository discountCodeRepository)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
            _businessRepository = businessRepository;
            _discountCodeRepository = discountCodeRepository;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.Today;
            var customersWithBirthday = await _customerRepository.GetCustomersWithUpcomingBirthdayAsync(today);

            foreach (var customer in customersWithBirthday)
            {
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    var business = await _businessRepository.GetByIdAsync(customer.BusinessId);
                    var businessName = business?.Name ?? "Our Business";

                    // Get the birthday discount percentage
                    var discountPercentage = await _businessRepository.GetBirthdayDiscountPercentageAsync(customer.BusinessId) ?? 0;

                    // If there's a birthday discount, generate a code
                    string? discountCode = null;
                    if (discountPercentage > 0)
                    {
                        discountCode = GenerateUniqueCode();

                        // Save the discount code in the database
                        var newDiscountCode = new DiscountCode
                        {
                            Code = discountCode,
                            DiscountPercentage = discountPercentage,
                            IsUsed = false,
                            GeneratedAt = DateTime.UtcNow
                        };
                        
                        await _discountCodeRepository.AddDiscountCodeAsync(newDiscountCode);
                    }

                    // Send the birthday email with discount code if applicable
                    bool isEmailSent = await _emailService.SendBirthdayEmailAsync(customer.Email, customer.Name, businessName, discountPercentage, discountCode);
                    
                    if (isEmailSent)
                    {
                        Console.WriteLine($"Birthday email sent to {customer.Name} from {businessName} with discount code {discountCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send birthday email to {customer.Name}");
                    }
                }
            }
        }

        private string GenerateUniqueCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }
}
