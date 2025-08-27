using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class Interlace3dDomainForwardingEvent
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phoneCountryCode")]
    public string PhoneCountryCode { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; }
}