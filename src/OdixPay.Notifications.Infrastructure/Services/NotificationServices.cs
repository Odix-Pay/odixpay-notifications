using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services;

public class NotificationService(
    IEmailService emailService,
    ISmsService smsService,
    IPushNotificationService pushNotificationService,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly ISmsService _smsService = smsService;
    private readonly IPushNotificationService _pushNotificationService = pushNotificationService;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task<SendNotificationResult> SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            return notification.Type switch
            {
                NotificationType.Email => await SendEmailAsync(notification, cancellationToken),
                NotificationType.SMS => await SendSmsAsync(notification, cancellationToken),
                NotificationType.Push => await SendPushNotificationAsync(notification, cancellationToken),
                NotificationType.InApp => await Task.FromResult<SendNotificationResult>(new()
                {
                    Success = true,
                    SentAt = DateTime.UtcNow
                }), // In-app notifications are stored in database
                _ => await Task.FromResult<SendNotificationResult>(new()
                {
                    Success = false,
                    ErrorMessage = $"Unsupported notification type: {notification.Type}",
                    SentAt = DateTime.UtcNow
                })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification {NotificationId} of type {NotificationType}",
                notification.Id, notification.Type);
            throw new Exception($"Failed to send notification {notification.Id} of type {notification.Type}: {ex.Message}", ex);
        }
    }

    public async Task<SendNotificationResult> SendEmailAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Recipient == null)
        {
            throw new ArgumentNullException(nameof(notification.Recipient), "Recipient cannot be null for email notifications.");
        }

        var to = notification.Recipient;
        var subject = notification.Title ?? "";
        var body = notification.Message ?? "No Message";

        return await _emailService.SendEmailAsync(new()
        {
            Data = notification.Data,
            Message = body,
            Recipient = to,
            Sender = notification.Sender,
            Title = subject
        }, cancellationToken);
    }

    public async Task<SendNotificationResult> SendSmsAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Recipient == null)
        {
            throw new ArgumentNullException(nameof(notification.Recipient), "Recipient cannot be null for SMS notifications.");
        }

        return await _smsService.SendSmsAsync(new()
        {
            Data = notification.Data,
            Message = notification.Message ?? "No Message",
            Recipient = notification.Recipient,
            Sender = notification.Sender,
            Title = notification.Title
        });
    }

    public async Task<SendNotificationResult> SendPushNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Recipient == null)
        {
            throw new ArgumentNullException(nameof(notification.Recipient), "Recipient cannot be null for push notifications.");
        }

        var to = notification.Recipient;
        var title = notification.Title ?? "";
        var message = notification.Message ?? "No Message";
        var data = notification.Data;

        return await _pushNotificationService.SendPushNotificationAsync(new()
        {
            Data = data,
            Message = message,
            Recipient = to,
            Sender = notification.Sender,
            Title = title
        }, cancellationToken);
    }

}
