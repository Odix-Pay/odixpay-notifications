using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Models;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string token);

    Task<AuthorizationResult> AuthorizeRoleAsync(string userId, string permission);
}