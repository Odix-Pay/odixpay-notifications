using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services;

public class RedisCacheService : IDistributedCacheService, IDisposable
{
    // private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IDatabase _database;
    private bool _isInitialized;

    public RedisCacheService(
        IConnectionMultiplexer redisConnection,
        ILogger<RedisCacheService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // Initialize Redis connection

        _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        _database = _redisConnection.GetDatabase();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            _logger.LogInformation("RedisCacheService already initialized");
            return;
        }

        try
        {
            // Test connection
            await _database.PingAsync().ConfigureAwait(false);
            _isInitialized = true;
            _logger.LogInformation("RedisCacheService initialized successfully");
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Failed to initialize Redis connection");
            throw;
        }
    }

    public async void SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        try
        {
            if (value == null)
            {
                _logger.LogWarning("Attempted to set null value for key {Key}", key);
                return;
            }

            RedisMessage<T> message = new()
            {
                Key = key,
                Value = value,
                Expiry = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(30))
            };

            var serializedValue = JsonSerializer.Serialize(message);

            cancellationToken.ThrowIfCancellationRequested();

            await _database.StringSetAsync(key, serializedValue, expiry, When.Always);

            _logger.LogDebug("Cached value for key {Key} with expiry {Expiry}", key, expiry);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key {Key}", key);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key {Key}", key);
                return default;
            }

            var result = JsonSerializer.Deserialize<RedisMessage<T>>(value);

            if (result == null)
            {
                _logger.LogDebug("Failed to deserialize cache value for key {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key {Key}", key);

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache for key {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cache for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key {Key}", key);
            throw;
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            await InitializeAsync(cancellationToken);
        }

        try
        {
            var endpoints = _redisConnection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redisConnection.GetServer(endpoint);
                if (server.IsConnected)
                {
                    await server.FlushDatabaseAsync();
                    _logger.LogInformation("Cleared cache for endpoint {Endpoint}", endpoint);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            throw;
        }
    }

    public void Dispose()
    {
        _redisConnection?.Dispose();
        _logger.LogInformation("RedisCacheService disposed");
    }
}

// Private methods for internal use can be added here if needed
// We want data to be saved in a specific format, so we can add custom serialization logic. Create a private class to handle serialization if needed.
class RedisMessage<T>
{
    public string Key { get; set; }
    public T Value { get; set; }
    public DateTimeOffset Expiry { get; set; }
}