using MassTransit;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Consumers;

public class PushNotificationConsumer : IConsumer<PushNotificationMessage>
{
    private readonly ILogger<PushNotificationConsumer> _logger;
    private readonly Random _random = new Random();

    public PushNotificationConsumer(ILogger<PushNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PushNotificationMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Processing push notification: {message.NotificationId} for recipient: {message.RecipientId}");

        bool isSuccessful = _random.Next(100) < 50;

        if (isSuccessful)
        {
           _logger.LogInformation($"Successfully delivered push notification: {message.NotificationId}. Content: {message.Content}");

           await context.Publish(new NotificationDeliveryResultMessage
           {
                NotificationId = message.NotificationId,
                IsSuccessful = true,
           });
        }
        else
        {
            _logger.LogWarning($"Failed to deliver push notification: {message.NotificationId}");

            await context.Publish(new NotificationDeliveryResultMessage
            {
                NotificationId = message.NotificationId,
                IsSuccessful = false,
                ErrorMessage = "Delivery failure"
            });
        }
    }
}