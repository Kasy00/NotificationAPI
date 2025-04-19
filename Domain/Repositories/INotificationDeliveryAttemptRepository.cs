using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Domain.Repositories;

public interface INotificationDeliveryAttemptRepository : IRepository<NotificationDeliveryAttempt>
{
    Task<IEnumerable<NotificationDeliveryAttempt>> GetAttemptsByNotificationId(Guid notificationId);
}