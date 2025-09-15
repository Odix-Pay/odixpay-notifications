using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.API.Auth;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

// Handler to check if the user has the required permission
public class PermissionAuthorizationHandler(IAuthenticationService authService, ILogger<PermissionAuthorizationHandler> logger, IMemoryCache cache) : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAuthenticationService _authzService = authService ?? throw new ArgumentNullException(nameof(authService));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<PermissionAuthorizationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {

        _logger.LogInformation("{User}", JsonSerializer.Serialize(context.User, new JsonSerializerOptions { WriteIndented = true }));
        _logger.LogInformation("Checking permission {Permission} for user {User}", requirement.Permission, context.User?.Identity?.Name ?? "Unknown");
        // Must be authenticated
        if (context.User?.Identity?.IsAuthenticated != true) return;

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId)) return;

        var cacheKey = $"perm:{userId}:{requirement.Permission}";

        if (!_cache.TryGetValue(cacheKey, out bool allowed))
        {

            var IsAuthorized = await _authzService.AuthorizeRoleAsync(userId, requirement.Permission);
            allowed = IsAuthorized.IsAuthorized;
            _logger.LogInformation("Authorization result for user {UserId} with permission {Permission}: {IsAuthorized}",
                userId, requirement.Permission, IsAuthorized.IsAuthorized);
            _cache.Set(cacheKey, allowed, TimeSpan.FromMinutes(2));
        }

        if (allowed)
            context.Succeed(requirement);
    }
}