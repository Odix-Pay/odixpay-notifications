using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OdixPay.Notifications.Infrastructure.Configuration;
using System.Data;

namespace OdixPay.Notifications.Infrastructure.Data;

public interface IConnectionFactory
{
    IDbConnection CreateConnection();
}

public class ConnectionFactory(IOptions<DbConfig> configuration) : IConnectionFactory
{
    private readonly DbConfig _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));

    public IDbConnection CreateConnection()
    {
        var connectionString = _configuration.ConnectionString;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"No connection string found in configuration.");
        }

        return new SqlConnection(connectionString);
    }
}