namespace NotificationSystem.Domain.Messages;

public class EmailNotificationMessage : INotificationMessage
{
    public Guid NotificationId { get; set; }
    public required string Content { get; set; }
    public required string RecipientId { get; set; }
    public required string TimeZone { get; set; }
}