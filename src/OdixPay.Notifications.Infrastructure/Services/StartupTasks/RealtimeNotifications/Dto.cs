using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.RealtimeNotifications;

public class RealtimeNotificationsDto
{
    [JsonPropertyName("topic")]
    public string? Topic { get; set; } // e.g., "admin.notification", "user.notification"

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
