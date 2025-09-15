using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.TransactionDetectionEvents;

public class BlockchainTransactionObserved
{
    [JsonPropertyName("network")]
    public string Network { get; set; }

    [JsonPropertyName("transactionHash")]
    public string TransactionHash { get; set; }
    [JsonPropertyName("contractAddress")]
    public string? ContractAddress { get; set; }
    [JsonPropertyName("fromAddress")]
    public string FromAddress { get; set; }
    [JsonPropertyName("toAddress")]
    public string ToAddress { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("isSuccessful")]
    public bool IsSuccessful { get; set; }
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
    [JsonPropertyName("direction")]
    public string? Direction { get; set; }
}