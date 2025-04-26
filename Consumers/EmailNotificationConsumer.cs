using MassTransit;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Consumers;

public class EmailNotificationConsumer : IConsumer<EmailNotificationMessage>
{
    private readonly ILogger<EmailNotificationConsumer> _logger;
    private readonly IMetricsService _metricsService;
    private readonly Random _random = new Random();
    private static readonly string ServerId = Guid.NewGuid().ToString();

    public EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger, IMetricsService metricsService)
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    public async Task Consume(ConsumeContext<EmailNotificationMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Processing email notification: {message.NotificationId} for recipient: {message.RecipientId}");

        bool isSuccessful = _random.Next(100) < 50;

        if (isSuccessful)
        {
           _logger.LogInformation($"Successfully delivered email notification: {message.NotificationId}. Content: {message.Content}");

           await context.Publish(new NotificationDeliveryResultMessage
           {
                NotificationId = message.NotificationId,
                IsSuccessful = true,
           });

           await _metricsService.RecordNotificationDelivered(ServerId, NotificationChannel.Email);
        }
        else
        {
            _logger.LogWarning($"Failed to deliver email notification: {message.NotificationId}");

            await context.Publish(new NotificationDeliveryResultMessage
            {
                NotificationId = message.NotificationId,
                IsSuccessful = false,
                ErrorMessage = "Delivery failure"
            });

            await _metricsService.RecordNotificationFailed(ServerId, NotificationChannel.Email);
        }
    }
}