namespace OdixPay.Notifications.Domain.Models;

public class AuthenticationResult
{
    public bool IsAuthenticated { get; set; } = false;
    public string? UserId { get; set; }
    public string? KeyId { get; set; }
    public string? PublicKey { get; set; }
    public string? Role { get; set; }
    public string? Region { get; set; }
    public string? MerchantId { get; set; }
    public string? ErrorMessage { get; set; } // Error message if authentication fails
}