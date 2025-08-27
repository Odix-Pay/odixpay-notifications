using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.Models;

/// <summary>
/// An envelope that holds message (data) to be published to other microservices
/// </summary>
/// <typeparam name="T">Dynamic data to be published</typeparam>
public class MessageEnvelope<T>(T? data)
{
    [JsonPropertyName("data")]
    public T? Data { get; set; } = data;

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}