namespace NotificationSystem.Domain.Entities;

public class NotificationDeliveryAttempt
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public DateTime AttemptTime { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}