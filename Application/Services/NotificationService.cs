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

    public NotificationService(IUnitOfWork unitOfWork, IBus bus)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
    }

    public async Task<Guid> CreateNotification(CreateNotificationDto createNotificationDto)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Content = createNotificationDto.Content,
            Channel = createNotificationDto.channel,
            TimeZone = createNotificationDto.TimeZone,
            RecipientId = createNotificationDto.RecipientId,
            ScheduledDeliveryTime = createNotificationDto.ScheduledDeliveryTime,
            Status = NotificationStatus.Created,
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

            notification.Status = NotificationStatus.Scheduled;
            
            await _unitOfWork.NotificationRepository.Update(notification);

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
            await ProcessNotification(notification);
        }

        foreach (var notification in failedNotifications)
        {
            await ProcessNotification(notification);
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
}