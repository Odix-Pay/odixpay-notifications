namespace OdixPay.Notifications.Domain.Models;

public class Permission
{
    public string Role { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}