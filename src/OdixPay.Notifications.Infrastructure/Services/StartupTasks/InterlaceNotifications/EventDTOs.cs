using System.Text.Json.Serialization;
using OdixPay.Notifications.Domain.DTO.Requests;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.InterlaceNotifications;

public class CardNotificationTemplateVariablesDTO
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class CardTransactionNotificationTemplateVariablesDTO
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

public class CardholderNotificationTemplateVariablesDTO
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class CardStatusChangedEvent : CreateNotificationBaseRequest
{
    [JsonPropertyName("templateVariables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CardNotificationTemplateVariablesDTO? TemplateVariables { get; set; }  // JSON with variable values for

}

public class CardTransactionStatusChangedEvent : CreateNotificationBaseRequest
{
    [JsonPropertyName("templateVariables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CardTransactionNotificationTemplateVariablesDTO? TemplateVariables { get; set; }  // JSON with variable values for
}


public class CardholderStatusChangedEvent : CreateNotificationBaseRequest
{
    [JsonPropertyName("templateVariables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CardholderNotificationTemplateVariablesDTO? TemplateVariables { get; set; }  // JSON with variable values for
}