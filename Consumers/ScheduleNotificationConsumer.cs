using MassTransit;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Consumers;

public class ScheduleNotificationConsumer : IConsumer<ScheduleNotificationMessage>
{
    private readonly ILogger<ScheduleNotificationConsumer> _logger;

    public ScheduleNotificationConsumer(ILogger<ScheduleNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ScheduleNotificationMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Scheduled notification: {message.NotificationId} for delivery at: {message.DeliveryTime}");
        
        return Task.CompletedTask;
    }
}