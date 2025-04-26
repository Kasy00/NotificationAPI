using MassTransit;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Consumers;

public class ScheduleNotificationConsumer : IConsumer<ScheduleNotificationMessage>
{
    private readonly ILogger<ScheduleNotificationConsumer> _logger;
    private readonly IMetricsService _metricsService;
    private readonly INotificationService _notificationService;
    private static readonly string ServerId = Guid.NewGuid().ToString();

    public ScheduleNotificationConsumer(ILogger<ScheduleNotificationConsumer> logger, IMetricsService metricsService, INotificationService notificationService)
    {
        _logger = logger;
        _metricsService = metricsService;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<ScheduleNotificationMessage> context)
    {
        var message = context.Message;
        var notification = await _notificationService.GetNotificationById(message.NotificationId);
        await _metricsService.RecordNotificationScheduled(ServerId, notification.Channel);
        _logger.LogInformation($"Scheduled notification: {message.NotificationId} for delivery at: {message.DeliveryTime}");
    }
}