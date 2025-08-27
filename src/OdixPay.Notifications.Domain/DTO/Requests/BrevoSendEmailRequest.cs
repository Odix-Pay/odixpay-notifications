using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class BrevoSendEmailRequest
{
    [JsonPropertyName("sender")]
    public BrevoEmailUser? Sender { get; set; }

    [JsonPropertyName("to")]
    public List<BrevoEmailUser> To { get; set; } = [];
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    [JsonPropertyName("htmlContent")]
    public string HtmlContent { get; set; } = string.Empty;
}

public class BrevoEmailUser
{
    [JsonPropertyName("name")]
    public string? Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
