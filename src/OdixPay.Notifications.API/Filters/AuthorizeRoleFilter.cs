using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.API.Filters;

public class AuthorizeRoleFilter() : Attribute, IAsyncAuthorizationFilter
{
    public string Permission { get; set; } = string.Empty;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {

        ArgumentNullException.ThrowIfNull(context);

        var _authService = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationService>() 
            ?? throw new InvalidOperationException("Authentication service is not registered.");

        var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthorizeRoleFilter>>() 
            ?? throw new InvalidOperationException("Logger service is not registered.");

        var _IStringLocalizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>()
            ?? throw new InvalidOperationException("Localizer service is not registered.");


        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = CreateUnauthorizedResult(_IStringLocalizer["UserNotAuthenticated"]);
            return;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            context.Result = CreateUnauthorizedResult(_IStringLocalizer["UserIDClaimIsMissingOrEmpty"]);
            return;
        }

        var userRoleId = user.FindFirst(ApiConstants.Headers.XUserRole)?.Value;

        if (string.IsNullOrEmpty(userRoleId))
        {
            context.Result = CreateUnauthorizedResult(_IStringLocalizer["UserIDClaimIsMissingOrEmpty"]);
            return;
        }

        if (string.IsNullOrEmpty(Permission))
        {
            context.Result = CreateBadRequestResult(_IStringLocalizer["PermissionMustBeSpecifiedInAuthorizeRoleFilter"]);
            return;
        }

        var authResult = await _authService.AuthorizeRoleAsync(userIdClaim.Value, Permission);

        _logger.LogInformation("Authorization result for user {UserId} with permission {Permission}: {IsAuthorized}",
            userIdClaim.Value, Permission, authResult.IsAuthorized);

        if (!authResult.IsAuthorized)
        {
            _logger.LogWarning("Authorization failed for user {UserId} with permission {Permission}: {ErrorMessage}",
                userIdClaim.Value, Permission, authResult.ErrorMessage);
            context.Result = CreateForbiddenResult(authResult.ErrorMessage ?? _IStringLocalizer["UserIsNotAuthorizedForTheRequestedAction"]);
            return;
        }

        // If we reach here, user is both authenticated and authorized - allow the request to continue
        _logger.LogInformation("Authorization successful for user {UserId} with permission {Permission}",
            userIdClaim.Value, Permission);
    }

    private IActionResult CreateUnauthorizedResult(string message)
    {
        return new UnauthorizedObjectResult(StandardResponse<object, string>.Error(
            error: ApiConstants.ErrorTypes.Unauthorized,
            message: message
        ));
    }

    private IActionResult CreateForbiddenResult(string message)
    {
        return new ObjectResult(StandardResponse<object, string>.Error(
            error: ApiConstants.ErrorTypes.Forbidden,
            message: message
        ))
        { StatusCode = StatusCodes.Status403Forbidden };
    }

    private IActionResult CreateBadRequestResult(string message)
    {
        return new BadRequestObjectResult(StandardResponse<object, string>.Error(
            error: ApiConstants.ErrorTypes.BadRequest,
            message: message
        ));
    }
}