namespace NotificationSystem.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public NotificationChannel Channel { get; set; }
    public required string TimeZone { get; set; }
    public required string RecipientId { get; set; }
    public DateTime ScheduledDeliveryTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}