using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Repositories;

namespace NotificationSystem.Infrastructure.Data;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(NotificationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Notification>> GetScheduledNotifications(DateTime beforeTime)
    {
        return await _dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Scheduled
                && n.ScheduledDeliveryTime <= beforeTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetFailedNotificationsForRetry()
    {
        return await _dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.RetryScheduled && n.RetryCount < 3)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByStatus(NotificationStatus status)
    {
        return await _dbContext.Notifications
            .Where(n => n.Status == status)
            .ToListAsync();
    }
}