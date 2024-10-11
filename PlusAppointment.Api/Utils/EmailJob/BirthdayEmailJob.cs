using PlusAppointment.Repositories.Interfaces.BusinessRepo;
using PlusAppointment.Repositories.Interfaces.CustomerRepo;
using PlusAppointment.Services.Interfaces.EmailSendingService;

namespace PlusAppointment.Utils.EmailJob
{
    public class BirthdayEmailJob
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IEmailService _emailService;

        public BirthdayEmailJob(ICustomerRepository customerRepository, IEmailService emailService, IBusinessRepository businessRepository)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
            _businessRepository = businessRepository;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.Today;
            var customersWithBirthday = await _customerRepository.GetCustomersWithUpcomingBirthdayAsync(today);

            foreach (var customer in customersWithBirthday)
            {
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    // Get the business by BusinessId
                    var business = await _businessRepository.GetByIdAsync(customer.BusinessId);
                    var businessName = business?.Name ?? "Our Business"; // Fallback if the business name is not available

                    // Send the birthday email with the business name
                    bool isEmailSent = await _emailService.SendBirthdayEmailAsync(customer.Email, customer.Name, businessName);
                    if (isEmailSent)
                    {
                        Console.WriteLine($"Birthday email sent to {customer.Name} from {businessName}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send birthday email to {customer.Name}");
                    }
                }
            }
        }
    }
}
