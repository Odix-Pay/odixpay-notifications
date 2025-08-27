using RabbitMQ.Client;

namespace OdixPay.Notifications.Infrastructure.Configuration;

public class RabbitMqConfig
{
    public string? Url { get; set; } // Full connection string
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int ConnectionTimeout { get; set; } = 30000;
    public int RetryCount { get; set; } = 5;
    public int RetryDelay { get; set; } = 2000;
    public bool UseSsl { get; set; } = false;

    public string ExchangeName { get; } = "odixpay-interlace-api";

    // Helper method to get connection factory
    public ConnectionFactory GetConnectionFactory()
    {
        var factory = new ConnectionFactory();

        // If URL is provided, use it (takes precedence)
        if (!string.IsNullOrEmpty(Url))
        {
            factory.Uri = new Uri(Url);
        }
        else
        {
            // Use individual properties
            factory.HostName = Host;
            factory.Port = Port;
            factory.UserName = Username;
            factory.Password = Password;
            factory.VirtualHost = VirtualHost;
        }

        factory.RequestedConnectionTimeout = TimeSpan.FromMilliseconds(ConnectionTimeout);
        factory.AutomaticRecoveryEnabled = true;
        factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);

        return factory;
    }
}