using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Services;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Controllers;

[Route("api/v0/[controller]")]
[ApiController]
public class MetricsController : ControllerBase
{   
    private readonly IMetricsService _metricsService;

    public MetricsController(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] NotificationChannel channel,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        var startDate = start ?? DateTime.UtcNow.AddDays(-7).Date;
        var endDate = end ?? DateTime.UtcNow.Date.AddDays(1);

        var metrics = await _metricsService.GetMetrics(startDate, endDate, channel);

        return Ok(metrics);
    }
}