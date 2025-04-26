using NotificationSystem.Domain.Dto;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Messages;

namespace NotificationSystem.Application.Services;

public interface INotificationService
{
    Task<Guid> CreateNotification(CreateNotificationDto createNotificationDto);
    Task ProcessScheduledNotifications();
    Task HandleDeliveryResult(NotificationDeliveryResultMessage resultMessage);
    Task ProcessOutboxMessages();
    Task<Notification> GetNotificationById(Guid id);
    Task<IEnumerable<Notification>> GetNotifications(string status = null);
}