namespace NotificationSystem.Domain.Entities;

public class NotificationMetrics
{
    public Guid Id { get; set; }
    public required string ServerId { get; set; }
    public NotificationChannel Channel { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int DeliveredCount { get; set; }
    public int ScheduledCount { get; set; }
    public int FailedCount { get; set; }
}