using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;


namespace WebApplication1.Utils.SendingSms
{
    public class SmsService
    {
        private readonly AmazonSimpleNotificationServiceClient _snsClient;

        public SmsService(IConfiguration configuration)
        {
            var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            var credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"]
            );

            _snsClient = new AmazonSimpleNotificationServiceClient(credentials, region);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            var request = new PublishRequest
            {
                Message = message,
                PhoneNumber = phoneNumber
            };

            try
            {
                var response = await _snsClient.PublishAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The SMS was not sent. Error message: {ex.Message}");
                return false;
            }
        }
    }
}