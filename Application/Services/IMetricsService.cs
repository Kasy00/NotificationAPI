using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Services;

public interface IMetricsService
{
    Task RecordNotificationDelivered(string serverId, NotificationChannel channel);
    Task RecordNotificationScheduled(string serverId, NotificationChannel channel);
    Task RecordNotificationFailed(string serverId, NotificationChannel channel);
    Task<IEnumerable<NotificationMetrics>> GetMetrics(DateTime start, DateTime end, NotificationChannel channel);
}