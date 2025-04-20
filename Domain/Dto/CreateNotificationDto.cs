using System.ComponentModel.DataAnnotations;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Domain.Dto;

public class CreateNotificationDto
{
    [Required]
    public required string Content { get; set; }
    [Required]
    [EnumDataType(typeof(NotificationChannel))]
    public NotificationChannel Channel { get; set; }
    [Required]
    public required string TimeZone { get; set; }
    [Required]
    public required string RecipientId { get; set; }
    [Required]
    public DateTime ScheduledDeliveryTime { get; set; } 
}