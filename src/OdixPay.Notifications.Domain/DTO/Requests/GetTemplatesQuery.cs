using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.DTO.Requests
{
    public class GetTemplatesQueryDTO : PaginationQueryParams
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("type")]
        [EnumDataType(typeof(NotificationType), ErrorMessage = "Invalid notification type.")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public NotificationType? Type { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("search")]
        public string? Search { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }
    }
}
