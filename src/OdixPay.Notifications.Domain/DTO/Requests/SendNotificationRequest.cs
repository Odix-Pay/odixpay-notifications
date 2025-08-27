namespace OdixPay.Notifications.Domain.DTO.Requests;

public class SendNotificationRequest
{
    public string? Sender { get; set; }
    public string Recipient { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Data { get; set; }
}
