using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Repositories;

namespace NotificationSystem.Infrastructure.Data;

public class NotificationDeliveryAttemptRepository : GenericRepository<NotificationDeliveryAttempt>, INotificationDeliveryAttemptRepository
{
    public NotificationDeliveryAttemptRepository(NotificationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<NotificationDeliveryAttempt>> GetAttemptsByNotificationId(Guid notificationId)
    {
        return await _dbContext.NotificationDeliveryAttempts
            .Where(a => a.NotificationId == notificationId)
            .ToListAsync();
    }
}