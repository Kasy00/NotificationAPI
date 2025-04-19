using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Repositories;

namespace NotificationSystem.Infrastructure.Data;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly NotificationDbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
    }

    public virtual async Task<T> GetById(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task Add(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual Task Update(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual async Task Delete(Guid id)
    {
        var entity = await GetById(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}