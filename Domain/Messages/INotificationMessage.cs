namespace NotificationSystem.Domain.Messages;

public interface INotificationMessage
{
    Guid NotificationId { get; }
    string Content { get; }
    string RecipientId { get; }
    string TimeZone { get; }
}

