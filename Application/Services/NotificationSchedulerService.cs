namespace NotificationSystem.Application.Services;

public class NotificationSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationSchedulerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.ProcessScheduledNotifications();
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}