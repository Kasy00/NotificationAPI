using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Domain.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetScheduledNotifications(DateTime beforeTime);
    Task<IEnumerable<Notification>> GetFailedNotificationsForRetry();
}