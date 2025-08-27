namespace OdixPay.Notifications.Domain.Interfaces;

public interface IDistributedCacheService : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    void SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}