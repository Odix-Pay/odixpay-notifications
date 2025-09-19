namespace OdixPay.Notifications.Infrastructure.Configuration;

public class RedisConfig
{
    public string InstanceName { get; set; } = "RedisInstance";
    // Connection properties
    public RedisConfiguration Configuration { get; set; } = new();


    // Helper method to get the full connection string
    public string GetConnectionString()
    {
        if (!string.IsNullOrEmpty(Configuration.Url))
        {
            return Configuration.Url;
        }

        var parts = new List<string>
        {
            $"host={Configuration.Host}",
            $"port={Configuration.Port}",
            $"connectTimeout={Configuration.ConnectTimeout}",
            $"syncTimeout={Configuration.ResponseTimeout}",
            $"asyncTimeout={Configuration.ResponseTimeout}"
        };

        if (!string.IsNullOrEmpty(Configuration.Password))
        {
            parts.Add($"password={Configuration.Password}");
        }

        if (Configuration.Ssl)
        {
            parts.Add("ssl=True");
            parts.Add("sslHost=" + Configuration.Host);
        }
        else
        {
            parts.Add("ssl=False");
        }

        return string.Join(";", parts);
    }

    public string GetMaskedConnectionString()
    {
        var connectionString = GetConnectionString();
        if (string.IsNullOrEmpty(Configuration.Password))
        {
            return connectionString;
        }

        return connectionString.Replace(Configuration.Password, "****");
    }

}
public class RedisConfiguration
{
    public string? Url { get; set; } // Full connection string, if provided, takes precedence
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public bool Ssl { get; set; } = false;
    public bool AbortConnect { get; set; } = false;
    public int ConnectTimeout { get; set; } = 5000; // in milliseconds
    public int ResponseTimeout { get; set; } = 5000; // in milliseconds
}