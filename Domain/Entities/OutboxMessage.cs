namespace NotificationSystem.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; }
    public string MessageContent { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}