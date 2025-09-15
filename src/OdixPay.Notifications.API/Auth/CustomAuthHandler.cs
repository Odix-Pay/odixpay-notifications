using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TSAuth = OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Contracts.Constants;

namespace OdixPay.Notifications.API.Auth;

public class CustomAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    ILogger<CustomAuthHandler> _logger,
    UrlEncoder encoder,
    TSAuth.IAuthenticationService authService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{

    private readonly TSAuth.IAuthenticationService _authService = authService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        // if (!Request.Headers.TryGetValue(APIConstants.Headers.Authorization, out var authHeader))
        // {
        //     Context.Items["AuthError"] = "Missing Authorization Header";

        //     return AuthenticateResult.Fail("Missing Authorization Header");
        // }

        // var token = authHeader.ToString();


        // 1) Try header: Authorization: Bearer <token>
        string? token = null;

        if (Request.Headers.TryGetValue(APIConstants.Headers.Authorization, out var authHeader) &&
            !string.IsNullOrWhiteSpace(authHeader))
        {
            var raw = authHeader.ToString();
            token = raw.Trim();
            // raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            //     ? raw.Substring("Bearer ".Length).Trim()
            //     : raw.Trim();
        }

        // 2) Fallback for SignalR WebSockets: ?access_token=<token>
        if (string.IsNullOrEmpty(token))
        {
            var qsToken = Request.Query["access_token"].ToString(); // important for client to send token as "Bearer {token}" in query string
            if (!string.IsNullOrWhiteSpace(qsToken))
                token = $"Bearer {qsToken}".Trim();
        }

        // 3) If still no token -> anonymous (NoResult)
        if (string.IsNullOrEmpty(token))
        {
            // Important: do NOT Fail here; returning NoResult enables anonymous flows.
            _logger.LogDebug("No token provided. Proceeding as anonymous.");
            return AuthenticateResult.NoResult();
        }

        try
        {
            var authResult = await _authService.AuthenticateAsync(token); // Ensures valid token and kyc status completed

            if (!authResult.IsAuthenticated)
            {
                Context.Items["AuthError"] = authResult?.ErrorMessage ?? "Invalid token";
                return AuthenticateResult.Fail(authResult?.ErrorMessage ?? "Invalid token");
            }

            var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, authResult.UserId!),
                new Claim(APIConstants.Headers.KeyId, authResult.KeyId!),
                new Claim(APIConstants.Headers.PublicKey, authResult.PublicKey!),
                new Claim(APIConstants.Headers.XUserRole, authResult.Role!),
            };

            // Set request header for user region
            if (!string.IsNullOrEmpty(authResult.Region?.Trim()))
            {
                claims = [.. claims, new Claim(APIConstants.Headers.XRegion, authResult.Region)];
            }

            if (!string.IsNullOrEmpty(authResult.MerchantId?.Trim()))
            {
                claims = [.. claims, new Claim("MerchantId", authResult.MerchantId.Trim())];
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed: {Message}", ex.Message);
            Context.Items["AuthError"] = ex.Message; // Store error message for challenge response
            return AuthenticateResult.Fail(ex.Message);
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        _logger.LogWarning("Authentication challenge triggered");
        // Customize the response on authentication failure (401)
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        // Retrieve the error message from the context
        // This can be set in the HandleAuthenticateAsync method when authentication fails
        // or you can set a default message if not present
        Context.Items.TryGetValue("AuthError", out var authError);
        // Use the error message if available, otherwise use a default message
        // This allows you to provide more context about the failure
        // If you want to use a specific error message, you can set it here
        // For example, you can set it in the HandleAuthenticateAsync method when authentication fails
        // or you can set a default message if not present

        var errorMessage = authError ?? "Unauthorized";

        var response = StandardResponse<object, object>.Error(
            new { reason = errorMessage },
            "Authentication failed"
        );

        await Response.WriteAsJsonAsync(response);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        // Customize the response on authorization failure (403)
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "application/json";

        var response = StandardResponse<object, object>.Error(new { reason = "Forbidden" }, "You do not have access to this resource");

        await Response.WriteAsJsonAsync(response);
    }
}