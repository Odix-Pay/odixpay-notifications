namespace OdixPay.Notifications.Infrastructure.Configuration;

public class RoleServiceHTTPClientConfig
{
    public string BaseUrl { get; set; }
    public int RetryCount { get; set; } = 2;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}