using NotificationSystem.Domain.Repositories;

namespace NotificationSystem.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly NotificationDbContext _dbContext;
    private INotificationRepository _notificationRepository;
    private INotificationDeliveryAttemptRepository _notificationDeliveryAttemptRepository;
    private IOutboxMessageRepository _outboxMessageRepository;
    private bool _disposed = false;

    public UnitOfWork(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public INotificationRepository NotificationRepository => 
        _notificationRepository ??= new NotificationRepository(_dbContext);
    
    public INotificationDeliveryAttemptRepository NotificationDeliveryAttemptRepository =>
        _notificationDeliveryAttemptRepository ??= new NotificationDeliveryAttemptRepository(_dbContext);

    public IOutboxMessageRepository OutboxMessageRepository =>
        _outboxMessageRepository ??= new OutboxMessageRepository(_dbContext);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _dbContext.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _dbContext.Database.RollbackTransactionAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}