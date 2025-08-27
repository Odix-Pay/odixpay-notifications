using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services.BackgroundTasks;

public class NotificationProcessorService(IServiceProvider serviceProvider, ILogger<NotificationProcessorService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<NotificationProcessorService> _logger = logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationProcessorService started at {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("=== Starting new processing cycle at {Time} ===", DateTimeOffset.Now);

                // Process scheduled notifications first
                await ProcessScheduledNotifications(stoppingToken);

                // Then check delivery statuses
                // await UpdateDeliveryStatuses(stoppingToken);

                _logger.LogInformation("Completed notification processing cycle. Next cycle in {Interval} seconds", _processingInterval.TotalSeconds);

                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("NotificationProcessorService is stopping gracefully");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notification processing cycle");
                // Continue processing even if there's an error, but wait before retrying
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }

        _logger.LogInformation("NotificationProcessorService stopped at {Time}", DateTimeOffset.Now);
    }

    private async Task ProcessScheduledNotifications(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>() ?? throw new InvalidOperationException("NotificationRepository is not registered");
            var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException("NotificationCommandHandler is not registered");

            // Get notifications that are scheduled and ready to be sent
            var scheduledNotifications = await GetReadyScheduledNotifications(notificationRepository, cancellationToken);

            if (scheduledNotifications.Any())
            {
                _logger.LogInformation("Found {Count} scheduled notifications ready for processing", scheduledNotifications.Count());

                foreach (var notification in scheduledNotifications)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await ProcessSingleScheduledNotification(notification, handler, notificationRepository, cancellationToken);
                }
            }
            else
            {
                _logger.LogDebug("No scheduled notifications found ready for processing");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled notifications");
        }
    }

    private async Task<IEnumerable<Notification>> GetReadyScheduledNotifications(INotificationRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            // Get notifications that:
            // 1. Have status Pending
            // 2. Have ScheduledAt time that is in the past or now
            // 3. Haven't exceeded retry limits
            var pendingNotifications = await repository.GetPendingNotificationsAsync(cancellationToken);

            var readyNotifications = pendingNotifications.Where(n =>
                (n.ScheduledAt == null ||
                n.ScheduledAt.HasValue &&
                n.ScheduledAt.Value <= DateTime.UtcNow) &&
                n.RetryCount < n.MaxRetries
            ).ToList();

            _logger.LogInformation("Found {Total} pending notifications, {Ready} are ready to send",
                pendingNotifications.Count(), readyNotifications.Count);

            return readyNotifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ready scheduled notifications");
            return Enumerable.Empty<Notification>();
        }
    }

    private async Task ProcessSingleScheduledNotification(
        Notification notification,
        INotificationCommandHandler handler,
        INotificationRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing scheduled notification {NotificationId} for user {UserId}, Type: {Type}",
                notification.Id, notification.UserId, notification.Type);

            // Send the notification
            var result = await handler.HandleSendNotificationAsync(notification.Id, cancellationToken);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled notification {NotificationId}", notification.Id);

            try
            {
                // Still increment retry count on exception
                await repository.IncrementRetryCountAsync(notification.Id, cancellationToken);

                // Mark as failed if max retries exceeded
                var currentRetryCount = notification.RetryCount + 1;
                if (currentRetryCount >= notification.MaxRetries)
                {
                    await repository.UpdateNotificationStatusAsync(notification.Id, NotificationStatus.Failed,
                        $"Processing error: {ex.Message}", cancellationToken);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update notification {NotificationId} after processing error", notification.Id);
            }
        }
    }

    private async Task UpdateDeliveryStatuses(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();

            // Get notifications that are sent but delivery status is unknown
            var sentNotifications = await GetNotificationsForStatusUpdate(notificationRepository, cancellationToken);

            if (sentNotifications.Any())
            {
                _logger.LogInformation("Checking delivery status for {Count} notifications", sentNotifications.Count());

                foreach (var notification in sentNotifications)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await UpdateSingleNotificationDeliveryStatus(notification, smsService, notificationRepository, cancellationToken);
                }
            }
            else
            {
                _logger.LogDebug("No notifications found requiring delivery status updates");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery statuses");
        }
    }

    private async Task<IEnumerable<Notification>> GetNotificationsForStatusUpdate(INotificationRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            // Get notifications that:
            // 1. Status is Sent
            // 2. Have ExternalId (for tracking)
            // 3. SentAt is within last 24 hours (don't check old notifications)
            // 4. Specific types that support delivery tracking (SMS primarily)
            // var sentNotifications = await repository.GetNotificationsByStatusAsync(NotificationStatus.Sent, cancellationToken);

            // var trackableNotifications = sentNotifications.Where(n =>
            //     !string.IsNullOrEmpty(n.ExternalId) &&
            //     n.SentAt.HasValue &&
            //     n.SentAt.Value > DateTime.UtcNow.AddHours(-24) &&
            //     n.Type == NotificationType.SMS // Currently only SMS supports delivery tracking
            // ).ToList();

            // _logger.LogDebug("Found {Total} sent notifications, {Trackable} are trackable for delivery status",
            //     sentNotifications.Count(), trackableNotifications.Count);

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for status update");
            return Enumerable.Empty<Notification>();
        }
    }

    private async Task UpdateSingleNotificationDeliveryStatus(
        Notification notification,
        ISmsService smsService,
        INotificationRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            if (notification.Type == NotificationType.SMS && !string.IsNullOrEmpty(notification.ExternalId))
            {
                var wasUpdated = await UpdateSmsDeliveryStatus(notification, smsService, repository, cancellationToken);

                if (wasUpdated)
                {
                    _logger.LogInformation("Updated delivery status for SMS notification {NotificationId} to {Status}",
                        notification.Id, notification.Status);
                }
            }
            // Future: Add email delivery status checking here
            // else if (notification.Type == NotificationType.Email && !string.IsNullOrEmpty(notification.ExternalId))
            // {
            //     await UpdateEmailDeliveryStatus(notification, emailService, repository, cancellationToken);
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery status for notification {NotificationId}", notification.Id);
        }
    }

    private async Task<bool> UpdateSmsDeliveryStatus(
        Notification notification,
        ISmsService smsService,
        INotificationRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Checking SMS delivery status for notification {NotificationId} with SID {ExternalId}",
                notification.Id, notification.ExternalId);

            // Get delivery status from Twilio
            // var deliveryStatus = await smsService.GetDeliveryStatusAsync(notification.ExternalId!, cancellationToken);

            // var previousStatus = notification.Status;
            // NotificationStatus? newStatus = null;
            // string? errorMessage = null;
            // DateTime? deliveredAt = null;

            // // Map Twilio status to our notification status
            // switch (deliveryStatus.Status?.ToLower())
            // {
            //     case "delivered":
            //         newStatus = NotificationStatus.Delivered;
            //         deliveredAt = deliveryStatus.DeliveredAt ?? DateTime.UtcNow;
            //         break;

            //     case "failed":
            //     case "undelivered":
            //         newStatus = NotificationStatus.Failed;
            //         errorMessage = deliveryStatus.ErrorMessage ?? "SMS delivery failed";
            //         break;

            //     case "sent":
            //     case "queued":
            //     case "accepted":
            //     case "sending":
            //         // Still in transit, no update needed
            //         _logger.LogDebug("SMS notification {NotificationId} still in transit with status: {Status}",
            //             notification.Id, deliveryStatus.Status);
            //         return false;

            //     default:
            //         _logger.LogWarning("Unknown SMS delivery status: {Status} for notification {NotificationId}",
            //             deliveryStatus.Status, notification.Id);
            //         return false;
            // }

            // // Update status if it changed
            // if (newStatus.HasValue && newStatus.Value != previousStatus)
            // {
            //     await repository.UpdateNotificationStatusAsync(notification.Id, newStatus.Value, errorMessage, cancellationToken);

            //     if (deliveredAt.HasValue)
            //     {
            //         await repository.UpdateNotificationDeliveredAsync(notification.Id, deliveredAt.Value, cancellationToken);
            //     }

            //     return true;
            // }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SMS delivery status for notification {NotificationId}", notification.Id);
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationProcessorService is stopping...");
        await base.StopAsync(stoppingToken);
        _logger.LogInformation("NotificationProcessorService stopped");
    }
}