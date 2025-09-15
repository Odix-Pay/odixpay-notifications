using Microsoft.Extensions.Caching.Memory;
using OdixPay.Notifications.Contracts.Interfaces;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services;

public class HubAuthorizer(IAuthenticationService authenticationService, IMemoryCache memoryCache) : IHubAuthorizer
{
    private readonly IAuthenticationService _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

    public async Task<bool> AuthorizeAsync(string userId, string permission)
    {
        var cacheKey = $"perm:{userId}:{permission}";
        
        if (_memoryCache.TryGetValue(cacheKey, out bool isAuthorized))
        {
            return isAuthorized;
        }

        var result = await _authenticationService.AuthorizeRoleAsync(userId, permission);

        isAuthorized = result.IsAuthenticated && result.IsAuthorized;

        _memoryCache.Set(cacheKey, isAuthorized, TimeSpan.FromMinutes(2));

        return isAuthorized;
    }
}