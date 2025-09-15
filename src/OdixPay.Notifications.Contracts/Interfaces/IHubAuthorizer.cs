namespace OdixPay.Notifications.Contracts.Interfaces;

public interface IHubAuthorizer
{
    Task<bool> AuthorizeAsync(string userId, string permission);
}
