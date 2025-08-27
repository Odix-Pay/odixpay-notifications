using System.Text.Json.Serialization;
using OdixPay.Notifications.Domain.DTO.Requests;

namespace OdixPay.Notifications.Domain.DTO.Events;

public class UserDataChangedEvent : CreateNotificationRecipientRequestDTO
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
}