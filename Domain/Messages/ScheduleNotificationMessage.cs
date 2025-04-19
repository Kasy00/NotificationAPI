namespace NotificationSystem.Domain.Messages;
public class ScheduleNotificationMessage
{
    public Guid NotificationId { get; set; }
    public DateTime DeliveryTime { get; set; }
}