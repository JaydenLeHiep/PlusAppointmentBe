using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace WebApplication1.Utils.SendingEmail
{
    public class EmailService
    {
        private readonly AmazonSimpleEmailServiceClient _client;

        public EmailService(IConfiguration configuration)
        {
            var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            var credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"]
            );

            _client = new AmazonSimpleEmailServiceClient(credentials, region);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = "hieplecoding@gmail.com",
                Destination = new Destination
                {
                    ToAddresses = new List<string> { toEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = body
                        },
                        Text = new Content
                        {
                            Charset = "UTF-8",
                            Data = body
                        }
                    }
                },
            };

            try
            {
                var response = await _client.SendEmailAsync(sendRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The email was not sent. Error message: {ex.Message}");
                return false;
            }
        }
    }
}
