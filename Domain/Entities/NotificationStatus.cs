namespace NotificationSystem.Domain.Entities;

public enum NotificationStatus
{
    Created,
    Scheduled,
    Processing,
    Delivered,
    Failed,
    RetryScheduled
}