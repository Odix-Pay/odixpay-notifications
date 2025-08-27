using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Infrastructure.Configuration;

namespace OdixPay.Notifications.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;
    private readonly FirebaseConfig _config;
    private readonly FirebaseMessaging _messaging;

    public PushNotificationService(ILogger<PushNotificationService> logger, IOptions<FirebaseConfig> config)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config), "FirebaseConfig cannot be null.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

        // Initialize Firebase if not already done
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(_config.ServiceAccountKeyPath),
                ProjectId = _config.ProjectId
            });
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<SendNotificationResult> SendPushNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            _logger.LogError("SendNotificationRequest is null.");
            throw new ArgumentNullException(nameof(request), "SendNotificationRequest cannot be null.");
        }

        var to = request.Recipient;
        var title = request.Title ?? string.Empty;
        var message = request.Message ?? string.Empty;
        var data = request.Data;
        var from = request.Sender ?? string.Empty;

        // Implement actual push notification logic here
        // For example, using Firebase Cloud Messaging or Apple Push Notification Service
        try
        {
            var msg = CreateMessage(to, title, message, JsonSerializer.Deserialize<Dictionary<string, object>>(data ?? "{}"));

            var response = await _messaging.SendAsync(msg, cancellationToken);
            _logger.LogInformation("Successfully sent push notification to {To} with response {Response}", to, response);

            return new SendNotificationResult
            {
                Success = true,
                SentAt = DateTime.UtcNow,
                ExternalId = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {To}", to);
            return new SendNotificationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                SentAt = DateTime.UtcNow
            };
        }
    }

    private Message CreateMessage(string token, string title, string body, Dictionary<string, object>? data = null)
    {
        var validData = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token), "Token cannot be null or empty.");
        // Ensure data is valid
        if (data != null)
        {
            foreach (var kvp in data)
            {
                validData[kvp.Key] = kvp.Value?.ToString() ?? string.Empty; // Ensure no null values
            }
        }

        return new Message()
        {
            Token = token,
            Notification = new Notification()
            {
                Title = title,
                Body = body,
                ImageUrl = _config.DefaultIconUrl
            },
            Data = validData,
            Android = new AndroidConfig()
            {
                Notification = new AndroidNotification()
                {
                    // Icon = "notification_icon",
                    // Color = "#FF6600",
                    // ClickAction = _config.DefaultClickAction,
                    Priority = NotificationPriority.DEFAULT,
                    Sound = "default"
                }
            },
            Apns = new ApnsConfig()
            {
                Aps = new Aps()
                {
                    Badge = 1,
                    Sound = "default"
                }
            }
        };
    }
}