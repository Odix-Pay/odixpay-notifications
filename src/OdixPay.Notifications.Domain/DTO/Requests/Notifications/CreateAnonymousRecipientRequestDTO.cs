using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace OdixPay.Notifications.Domain.DTO.Requests.Notifications
{
    public class CreateAnonymousRecipientRequestDTO : CreateNotificationRecipientRequestDTO
    {
        [JsonPropertyName("userId")]
        [Required(ErrorMessage = "UserId is required")]
        [MinLength(1, ErrorMessage = "At least one UserId must be provided")]
        public string UserId { get; set; }
    }
}
