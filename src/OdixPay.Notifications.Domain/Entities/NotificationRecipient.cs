using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.Entities;

public class NotificationRecipient : BaseEntity
{
    public string UserId { get; set; }
    public string Recipient { get; set; } // Value of the notification token (email for email notification, phone number for SMS notification, push notification token for push notifications, etc.)
    public NotificationType Type { get; set; } // Defines the token type (Email, SMS, PUSH, etc.)

    public string? Name { get; set; }
}

// NOTE: For simplicity, a single user cannot have multiple notification tokens of the same type.
// If a user tries to add a new token of an existing type, the old token will be replaced.