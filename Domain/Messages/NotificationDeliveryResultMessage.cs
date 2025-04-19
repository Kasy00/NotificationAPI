namespace NotificationSystem.Domain.Messages;

public class NotificationDeliveryResultMessage
{
    public Guid NotificationId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}