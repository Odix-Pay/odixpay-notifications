namespace OdixPay.Notifications.Domain.Models
{
    public class AuthorizationResult
    {
        public bool IsAuthenticated { get; set; }
        public bool IsAuthorized { get; set; }
        public string? ErrorMessage { get; set; }

        public static AuthorizationResult Success() => new() { IsAuthenticated = true, IsAuthorized = true };
        public static AuthorizationResult Unauthorized(string message) => new() { IsAuthenticated = false, IsAuthorized = false, ErrorMessage = message };
        public static AuthorizationResult Forbidden(string message) => new() { IsAuthenticated = true, IsAuthorized = false, ErrorMessage = message };
    }
}