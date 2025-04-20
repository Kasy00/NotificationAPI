using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Dto;

namespace NotificationSystem.Application.Controllers;

[Route("api/v0/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
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