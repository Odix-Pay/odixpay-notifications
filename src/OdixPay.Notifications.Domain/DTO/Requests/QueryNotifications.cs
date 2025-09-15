using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OdixPay.Notifications.Domain.Constants;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class QueryNotifications : PaginationQueryParams
{
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [EnumDataType(typeof(NotificationStatus), ErrorMessage = "Invalid notification status.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NotificationStatus? Status { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [EnumDataType(typeof(NotificationType), ErrorMessage = "Invalid notification type.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NotificationType? Type { get; set; }

    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("priority")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [EnumDataType(typeof(NotificationPriority), ErrorMessage = "Invalid notification priority.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NotificationPriority? Priority { get; set; }

    [JsonPropertyName("recipient")]
    public string? Recipient { get; set; }

    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    [JsonPropertyName("templateId")]
    public Guid? TemplateId { get; set; }

    [JsonPropertyName("search")]
    public string? Search { get; set; }

    [JsonPropertyName("isRead")]
    public bool? IsRead { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}
