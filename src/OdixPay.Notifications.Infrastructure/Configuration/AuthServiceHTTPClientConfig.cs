namespace OdixPay.Notifications.Infrastructure.Configuration;

public class AuthServiceHTTPClientConfig
{
    public string BaseUrl { get; set; }
    public int RetryCount { get; set; } = 2;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}