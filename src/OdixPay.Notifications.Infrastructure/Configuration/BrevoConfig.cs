namespace OdixPay.Notifications.Infrastructure.Configuration;

public class BrevoConfig
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public string DefaultSender { get; set; }
    public string DefaultSenderName { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30); // Default timeout in seconds
}
