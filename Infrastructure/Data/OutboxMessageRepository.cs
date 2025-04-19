using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Repositories;

namespace NotificationSystem.Infrastructure.Data;

public class OutboxMessageRepository : GenericRepository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(NotificationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessages()
    {
        return await _dbContext.OutboxMessages
            .Where(m => !m.IsProcessed)
            .ToListAsync();
    }

    public async Task MarkAsProcessed(Guid id)
    {
        var message = await GetById(id);
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            await Update(message);
        }
    }
}