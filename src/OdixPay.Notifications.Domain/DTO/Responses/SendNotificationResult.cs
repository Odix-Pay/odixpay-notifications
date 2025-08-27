namespace OdixPay.Notifications.Domain.DTO.Responses;

public class SendNotificationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? SentAt { get; set; }
}
