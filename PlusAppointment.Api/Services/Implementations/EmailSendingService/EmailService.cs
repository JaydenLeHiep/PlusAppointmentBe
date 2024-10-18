using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using PlusAppointment.Services.Interfaces.EmailSendingService;

namespace PlusAppointment.Services.Implementations.EmailSendingService
{
    public class EmailService : IEmailService
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

        public async Task<bool> SendEmailAsync(string toEmail, string? subject, string? body)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = "contactus@plus-appointment.com",
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
        // New bulk email sending method
        public async Task<bool> SendBulkEmailAsync(List<string?> toEmails, string? subject, string? body)
        {
            bool isAnyEmailSent = false;

            foreach (var toEmail in toEmails)
            {
                if (toEmail != null)
                {
                    try
                    {
                        var sendRequest = new SendEmailRequest
                        {
                            Source = "contactus@plus-appointment.com",
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

                        // Send the email using SES client
                        var response = await _client.SendEmailAsync(sendRequest);
                        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            isAnyEmailSent = true;
                        }
                        else
                        {
                            Console.WriteLine($"Failed to send email to {toEmail}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email to {toEmail}. Error message: {ex.Message}");
                    }
                }
            }

            return isAnyEmailSent;
        }
        public async Task<bool> SendBirthdayEmailAsync(string toEmail, string? name, string businessName)
        {
            string subject = $"Happy Birthday, {name}!";
            string body = $"<p>🎉 Dear {name},</p>" +
                          $"<p>{businessName} is sending you the biggest birthday wishes! May your day be filled with love, laughter, and everything that makes you smile! 🎂✨</p>" +
                          $"<p>Have an amazing year ahead!</p>" +
                          $"<br>" +
                          $"<p>🎉 Liebe/r {name},</p>" +
                          $"<p>{businessName} schickt Ihnen die herzlichsten Geburtstagswünsche! Möge Ihr Tag voller Liebe, Lachen und allem sein, was Sie glücklich macht! 🎂✨</p>" +
                          $"<p>Auf ein fantastisches neues Lebensjahr!</p>";

            return await SendEmailAsync(toEmail, subject, body);
        }



    }
}
