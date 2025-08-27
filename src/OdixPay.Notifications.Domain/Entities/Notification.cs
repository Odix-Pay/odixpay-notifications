using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Utils;

namespace OdixPay.Notifications.Domain.Entities;

public class Notification : BaseEntity
{
    public string? UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
    public NotificationStatus Status { get; set; }
    public NotificationPriority Priority { get; set; }
    public string? Recipient { get; set; }
    public string? Sender { get; set; } // Optional sender field
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public string? ExternalId { get; set; }

    public bool IsRead { get; set; } = false;

    // Template-related properties
    public Guid? TemplateId { get; set; }  // Reference to template
    public string? TemplateVariables { get; set; }  // JSON with variable values for template
}


