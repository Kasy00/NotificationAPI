using NotificationSystem.Domain.Dto;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Application.Services;

public interface INotificationService
{
    Task<Guid> CreateNotification(CreateNotificationDto createNotificationDto);
    Task ProcessScheduledNotifications();
    Task HandleDeliveryResult(NotificationDeliveryResultMessage resultMessage);
    Task ProcessOutboxMessages();
}