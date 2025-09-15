using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Domain.Interfaces;
using System.Reflection;

namespace OdixPay.Notifications.Infrastructure.Filters;

/// <summary>
/// A SignalR Hub Filter that enforces authorization policies on hub methods.
/// This ensures that [Authorize] attributes are correctly evaluated for every invocation.
/// </summary>
public class HubAuthorizationFilter(ILogger<HubAuthorizationFilter> logger, IAuthenticationService authenticationService) : IHubFilter
{
    private readonly ILogger<HubAuthorizationFilter> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IAuthenticationService _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        _logger.LogInformation("Invoking hub method '{MethodName}' for user '{User}'",
            invocationContext.HubMethodName,
            invocationContext.Context.UserIdentifier ?? "Anonymous");

        var authorizeAttribute = invocationContext.HubMethod.GetCustomAttribute<AuthorizeAttribute>();

        // If the method has no [Authorize] attribute, just continue
        if (authorizeAttribute == null)
        {
            return await next(invocationContext);
        }

        // Get the policy name from the attribute
        var policyName = authorizeAttribute.Policy;
        if (string.IsNullOrEmpty(policyName))
        {
            _logger.LogError("Hub method '{MethodName}' has an [Authorize] attribute but no policy is specified.", invocationContext.HubMethodName);
            throw new InvalidOperationException("Authorization policy must be specified on hub methods.");
        }

        // Ensure the user is authenticated
        if (invocationContext.Context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("Unauthenticated user attempted to invoke hub method '{Method}'.", invocationContext.HubMethodName);
            throw new HubException($"Unauthorized to invoke '{invocationContext.HubMethodName}'.");
        }

        var userId = invocationContext.Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("User without a valid identifier attempted to invoke hub method '{Method}'.", invocationContext.HubMethodName);
            throw new HubException($"Unauthorized to invoke '{invocationContext.HubMethodName}'.");
        }

        // Authorize against the policy
        var authorizationResult = await _authenticationService.AuthorizeRoleAsync(userId, policyName);

        if (authorizationResult.IsAuthenticated && authorizationResult.IsAuthorized)
        {
            _logger.LogInformation("Authorization succeeded for user '{User}' on hub method '{Method}'.", invocationContext.Context.UserIdentifier, invocationContext.HubMethodName);
            return await next(invocationContext);
        }
        else
        {
            // If authorization fails, log it and throw an exception that the client can handle.
            _logger.LogWarning("Authorization failed for user '{User}' on hub method '{Method}'. Policy: {Policy}",
                invocationContext.Context.UserIdentifier, invocationContext.HubMethodName, policyName);
            throw new HubException($"Unauthorized to invoke '{invocationContext.HubMethodName}'.");
        }
    }
}