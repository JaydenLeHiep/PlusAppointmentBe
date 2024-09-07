using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;

namespace PlusAppointment.Utils.SQS;

public class SqsProducer
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;

    public SqsProducer(IConfiguration configuration)
    {
        var serviceUrl = configuration["AWS:ServiceURL"];
        var region = configuration["AWS:Region"];
        var queueUrl = configuration["AWS:SQSQueueUrl"];

        var sqsConfig = new AmazonSQSConfig { ServiceURL = serviceUrl }; // Use LocalStack in dev
        _sqsClient = new AmazonSQSClient(sqsConfig);

        _queueUrl = queueUrl;
    }

    public async Task SendMessageAsync(string messageBody)
    {
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageBody
        };

        await _sqsClient.SendMessageAsync(sendMessageRequest);
    }
}