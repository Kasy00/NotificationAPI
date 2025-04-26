using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Dto;

namespace NotificationSystem.Application.Controllers;

[ApiController]
[Route("api/v0/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("notifications/{id}")]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        var notification = await _notificationService.GetNotificationById(id);

        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications([FromQuery] string status = null)
    {
        var notifications = await _notificationService.GetNotifications(status);
        return Ok(notifications);
    }

    [HttpPost("notifications")]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var notificationId = await _notificationService.CreateNotification(createNotificationDto);

        return Ok(new { NotificationId = notificationId });
    }
}