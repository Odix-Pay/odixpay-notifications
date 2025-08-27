using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Responses.AuthService;

public class GetPublicKeyResponse
{
    [JsonPropertyName("keyId")]
    public string KeyId { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string PublicKey { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = string.Empty;

    [JsonPropertyName("merchantId")]
    public string? MerchantId { get; set; } = string.Empty;
}