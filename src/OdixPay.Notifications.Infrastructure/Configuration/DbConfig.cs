namespace OdixPay.Notifications.Infrastructure.Configuration;

public class DbConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan MinBackoff { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromSeconds(15);
}