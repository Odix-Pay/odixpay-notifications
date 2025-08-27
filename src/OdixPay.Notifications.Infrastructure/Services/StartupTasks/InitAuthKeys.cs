using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks;

public class InitPublickeys(ILogger<InitPublickeys> logger, IServiceProvider serviceProvider) : IHostedService
{
    private readonly ILogger<InitPublickeys> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var authenticationService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        var authClient = scope.ServiceProvider.GetRequiredService<IAuthServiceClient>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCacheService>();

        _logger.LogInformation("Initialising public keys");

        try
        {
            var result = await authClient.QueryPublicKeysAsync([]);

            // Loop through the data and set in cache
            foreach (var item in result.Data)
            {
                var cacheKey = $"PublicKey_{item.KeyId}";
                cache.SetAsync(cacheKey, item, null, cancellationToken);
            }

            _logger.LogInformation("Public keys initialized successfully.");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Timeout occurred while fetching public keys. AuthService may be slow to respond.");
            // Don't throw - let application continue
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize public keys. Ensure the AuthService is running and accessible. {Mesage}", ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
