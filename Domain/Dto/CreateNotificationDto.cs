using System.ComponentModel.DataAnnotations;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Domain.Dto;

public class CreateNotificationDto
{
    public required string Content { get; set; }
    [EnumDataType(typeof(NotificationChannel))]
    public NotificationChannel Channel { get; set; }
    public required string TimeZone { get; set; }
    public required string RecipientId { get; set; }
    public DateTime ScheduledDeliveryTime { get; set; } 
}