using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    [EnumMember(Value = "Email")]
    Email = 1,
    [EnumMember(Value = "SMS")]
    SMS = 2,
    [EnumMember(Value = "Push")]
    Push = 3,
    [EnumMember(Value = "InApp")]
    InApp = 4
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}
