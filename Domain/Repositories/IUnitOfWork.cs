namespace NotificationSystem.Domain.Repositories;

public interface IUnitOfWork
{
    INotificationRepository NotificationRepository { get; }
    INotificationDeliveryAttemptRepository NotificationDeliveryAttemptRepository { get; }
    IOutboxMessageRepository OutboxMessageRepository { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}