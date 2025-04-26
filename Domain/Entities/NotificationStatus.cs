namespace NotificationSystem.Domain.Entities;

public enum NotificationStatus
{
    Scheduled,
    Processing,
    Delivered,
    Failed,
    RetryScheduled
}