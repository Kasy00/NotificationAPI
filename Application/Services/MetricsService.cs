using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Infrastructure.Data;

namespace NotificationSystem.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(NotificationDbContext dbContext, ILogger<MetricsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RecordNotificationDelivered(string serverId, NotificationChannel channel)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var metrics = await GetOrCreateMetrics(serverId, channel, today);
            metrics.DeliveredCount++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania metryki wysłanego powiadomienia");
        }
    }

    public async Task RecordNotificationScheduled(string serverId, NotificationChannel channel)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var metrics = await GetOrCreateMetrics(serverId, channel, today);
            metrics.ScheduledCount++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania metryki zaplanowanego powiadomienia");
        }
    }

    public async Task RecordNotificationFailed(string serverId, NotificationChannel channel)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var metrics = await GetOrCreateMetrics(serverId, channel, today);
            metrics.FailedCount++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania metryki nieudanego powiadomienia");
        }
    }

    public async Task<IEnumerable<NotificationMetrics>> GetMetrics(DateTime start, DateTime end, NotificationChannel channel)
    {
        return await _dbContext.NotificationMetrics
            .Where(m => m.PeriodStart >= start 
                && m.PeriodEnd <= end
                && m.Channel == channel)
            .ToListAsync();
    }

    private async Task<NotificationMetrics> GetOrCreateMetrics(string serverId, NotificationChannel channel, DateTime date)
    {
        var metrics = await _dbContext.NotificationMetrics
            .FirstOrDefaultAsync(m => m.ServerId == serverId && m.Channel == channel && m.PeriodStart.Date == date);

        if (metrics == null)
        {
            metrics = new NotificationMetrics
            {
                Id = Guid.NewGuid(),
                ServerId = serverId,
                Channel = channel,
                PeriodStart = date,
                PeriodEnd = date.AddDays(1).AddTicks(-1),
                DeliveredCount = 0,
                ScheduledCount = 0,
                FailedCount = 0
            };

            _dbContext.NotificationMetrics.Add(metrics);
        }

        return metrics;
    }
}