using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Domain.Repositories;

public interface IOutboxMessageRepository : IRepository<OutboxMessage>
{
    Task<IEnumerable<OutboxMessage>> GetUnprocessedMessages();
    Task MarkAsProcessed(Guid id);
}