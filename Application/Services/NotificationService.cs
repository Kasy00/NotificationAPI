using MassTransit;
using Newtonsoft.Json;
using NotificationSystem.Domain.Dto;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Messages;
using NotificationSystem.Domain.Repositories;

namespace NotificationSystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBus _bus;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, IBus bus, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _logger = logger;
    }

    public async Task<Guid> CreateNotification(CreateNotificationDto createNotificationDto)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Content = createNotificationDto.Content,
            Channel = createNotificationDto.Channel,
            TimeZone = createNotificationDto.TimeZone,
            RecipientId = createNotificationDto.RecipientId,
            ScheduledDeliveryTime = createNotificationDto.ScheduledDeliveryTime,
            Status = NotificationStatus.Scheduled,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.NotificationRepository.Add(notification);

            var scheduledMessage = new ScheduleNotificationMessage
            {
                NotificationId = notification.Id,
                DeliveryTime = createNotificationDto.ScheduledDeliveryTime
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = nameof(ScheduleNotificationMessage),
                MessageContent = JsonConvert.SerializeObject(scheduledMessage),
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OutboxMessageRepository.Add(outboxMessage);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return notification.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task HandleDeliveryResult(NotificationDeliveryResultMessage resultMessage)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var notification = await _unitOfWork.NotificationRepository.GetById(resultMessage.NotificationId);
            if (notification == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return;
            }

            var attempt = new NotificationDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                AttemptTime = DateTime.UtcNow,
                IsSuccessful = resultMessage.IsSuccessful,
                ErrorMessage = resultMessage.ErrorMessage
            };

            await _unitOfWork.NotificationDeliveryAttemptRepository.Add(attempt);

            if (resultMessage.IsSuccessful)
            {
                notification.Status = NotificationStatus.Delivered;
                notification.ActualDeliveryTime = DateTime.UtcNow;
            }
            else
            {
                notification.RetryCount++;

                if (notification.RetryCount >= 3)
                {
                    notification.Status = NotificationStatus.Failed;
                }
                else
                {
                    notification.Status = NotificationStatus.RetryScheduled;
                    notification.ScheduledDeliveryTime = DateTime.UtcNow.AddSeconds(Math.Pow(5, notification.RetryCount));
                }
            }

            notification.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.NotificationRepository.Update(notification);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ProcessOutboxMessages()
    {
        var unprocessedMessages = await _unitOfWork.OutboxMessageRepository.GetUnprocessedMessages();

        foreach (var message in unprocessedMessages)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                switch (message.MessageType)
                {
                    case nameof(PushNotificationMessage):
                        var pushMessage = JsonConvert.DeserializeObject<PushNotificationMessage>(message.MessageContent);
                        if (pushMessage == null)
                            throw new InvalidOperationException("Failed to deserialize PushNotificationMessage.");
                        await _bus.Publish(pushMessage);
                        break;
                    case nameof(EmailNotificationMessage):
                        var emailMesage = JsonConvert.DeserializeObject<EmailNotificationMessage>(message.MessageContent);
                        if (emailMesage == null)
                            throw new InvalidOperationException("Failed to deserialize EmailNotificationMessage.");
                        await _bus.Publish(emailMesage);
                        break;
                    case nameof(ScheduleNotificationMessage):
                        var scheduleMessage = JsonConvert.DeserializeObject<ScheduleNotificationMessage>(message.MessageContent);
                        if (scheduleMessage == null)
                            throw new InvalidOperationException("Failed to deserialize ScheduleNotificationMessage.");
                        await _bus.Publish(scheduleMessage);
                        break;
                }

                await _unitOfWork.OutboxMessageRepository.MarkAsProcessed(message.Id);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }
    }

    public async Task ProcessScheduledNotifications()
    {
        var scheduledNotifications = await _unitOfWork.NotificationRepository.GetScheduledNotifications(DateTime.UtcNow);
        var failedNotifications = await _unitOfWork.NotificationRepository.GetFailedNotificationsForRetry();

        foreach (var notification in scheduledNotifications)
        {
            if (IsAppropriateTimeToSend(notification))
            {
                await ProcessNotification(notification);
            }
            else
            {
                await RescheduleForAppropriateTime(notification);
            }
        }

        foreach (var notification in failedNotifications)
        {
            if (IsAppropriateTimeToSend(notification))
            {
                await ProcessNotification(notification);
            }
            else
            {
                await RescheduleForAppropriateTime(notification);
            }
        }
    }

    private async Task ProcessNotification(Notification notification)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            notification.Status = NotificationStatus.Processing;
            notification.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.NotificationRepository.Update(notification);

            INotificationMessage notificationMessage;
            string messageType;

            if (notification.Channel == NotificationChannel.Push)
            {
                notificationMessage = new PushNotificationMessage
                {
                    NotificationId = notification.Id,
                    Content = notification.Content,
                    RecipientId = notification.RecipientId,
                    TimeZone = notification.TimeZone                    
                };
                messageType = nameof(PushNotificationMessage);
            }
            else
            {
                notificationMessage = new EmailNotificationMessage
                {
                    NotificationId = notification.Id,
                    Content = notification.Content,
                    RecipientId = notification.RecipientId,
                    TimeZone = notification.TimeZone  
                };
                messageType = nameof(EmailNotificationMessage);
            }

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = messageType,
                MessageContent = JsonConvert.SerializeObject(notificationMessage),
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OutboxMessageRepository.Add(outboxMessage);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Notification> GetNotificationById(Guid id)
    {
        var notification = await _unitOfWork.NotificationRepository.GetById(id);

        if (notification == null)
        {
            return null;
        }

        return notification;
    }

    public async Task<IEnumerable<Notification>> GetNotifications(string status = null)
    {
        IEnumerable<Notification> notifications;

        if (string.IsNullOrEmpty(status))
        {
            notifications = await _unitOfWork.NotificationRepository.GetAll();
        }
        else
        {
            if (Enum.TryParse<NotificationStatus>(status, true, out var notificationStatus))
            {
                notifications = await _unitOfWork.NotificationRepository.GetByStatus(notificationStatus);
            }
            else
            {
                return Enumerable.Empty<Notification>();
            }
        }

        return notifications;
    }

    private bool IsAppropriateTimeToSend(Notification notification)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(notification.TimeZone);
            var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            var hour = userLocalTime.Hour;
            return hour >= 7 && hour < 22;
        }
        catch (TimeZoneNotFoundException)
        {
            _logger.LogWarning($"Nieprawidłowa strefa czasowa: {notification.TimeZone} dla powiadomienia ID: {notification.Id}");
            return true;
        }
    }

    private async Task RescheduleForAppropriateTime(Notification notification)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(notification.TimeZone);
            var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            DateTime nextAppropriateTime;
            if (userLocalTime.Hour < 7)
            {
                nextAppropriateTime = new DateTime(userLocalTime.Year, userLocalTime.Month, userLocalTime.Day, 7, 0, 0);
            }
            else
            {
                nextAppropriateTime = new DateTime(userLocalTime.Year, userLocalTime.Month, userLocalTime.Day, 7, 0, 0).AddDays(1);
            }

            var nextAppropriateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(nextAppropriateTime, timeZoneInfo);

            notification.ScheduledDeliveryTime = nextAppropriateTimeUtc;
            
            await _unitOfWork.NotificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Powiadomienie {notification.Id} przełożone na {nextAppropriateTimeUtc} UTC (lokalne: {nextAppropriateTime})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Błąd podczas przełożenia powiadomienia {notification.Id}");
        }
        
    }
}