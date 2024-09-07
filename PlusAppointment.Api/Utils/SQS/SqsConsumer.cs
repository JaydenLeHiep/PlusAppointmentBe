using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using PlusAppointment.Models.Classes;
using PlusAppointment.Utils.SendingEmail;

namespace PlusAppointment.Utils.SQS;

public class SqsConsumer
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IEmailService _emailService;
    private readonly string _queueUrl;

    public SqsConsumer(IConfiguration configuration, IEmailService emailService)
    {
        _emailService = emailService;

        var serviceUrl = configuration["AWS:ServiceURL"];
        var region = configuration["AWS:Region"];
        var queueUrl = configuration["AWS:SQSQueueUrl"];

        var sqsConfig = new AmazonSQSConfig { ServiceURL = serviceUrl }; // LocalStack in dev
        _sqsClient = new AmazonSQSClient(sqsConfig);

        _queueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl), "Queue URL cannot be null.");

    }

    // Hangfire job to consume and process emails from SQS
    public async Task ProcessEmailQueueAsync()
    {
        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 5 // Long-polling to minimize API requests
        };

        var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

        foreach (var message in response.Messages)
        {
            var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message.Body);

            // Send the email using your existing EmailService
            await _emailService.SendEmailAsync(emailMessage.ToEmail, emailMessage.Subject, emailMessage.Body);

            // Delete the message from the queue after processing
            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = message.ReceiptHandle
            });
        }
    }
}