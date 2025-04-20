using MassTransit;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Consumers;

public class NotificationDeliveryResultConsumer : IConsumer<NotificationDeliveryResultMessage>
{
    private readonly ILogger<NotificationDeliveryResultConsumer> _logger;
    private readonly INotificationService _notificationService;

    public NotificationDeliveryResultConsumer(ILogger<NotificationDeliveryResultConsumer> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<NotificationDeliveryResultMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Processing delivery result for notification: {message.NotificationId}, Success: {message.IsSuccessful}");


        await _notificationService.HandleDeliveryResult(message);
    }
}