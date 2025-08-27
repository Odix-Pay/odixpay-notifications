using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Responses;

public class BrevoSendEmailResponse
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; }
}
