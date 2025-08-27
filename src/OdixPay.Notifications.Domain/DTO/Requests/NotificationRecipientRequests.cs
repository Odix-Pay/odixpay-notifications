using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class CreateNotificationRecipientRequestDTO : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // If Type is email, then ensure Value is a valid email. If Type os SMS, then ensure Value is a valid phone number and if type is PUSH, then ensure Value is a push notification. Anything else is invalid.
        if (Type == NotificationType.Email)
        {
            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(Value))
            {
                yield return new ValidationResult("Invalid email address.", [nameof(Value)]);
            }
        }
        else if (Type == NotificationType.SMS)
        {
            var isValidPhoneNumber = true;
            var message = string.Empty;
            try
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var parsedNumber = phoneNumberUtil.Parse(Value, null);

                if (!phoneNumberUtil.IsValidNumber(parsedNumber))
                {
                    isValidPhoneNumber = false;
                    message = "Recipient must be a valid phone number.";
                }
            }
            catch (PhoneNumbers.NumberParseException)
            {
                isValidPhoneNumber = false;
                message = "Recipient must be a valid phone number.";
            }

            if (!isValidPhoneNumber)
            {
                yield return new ValidationResult(message, [nameof(Value)]);
            }

        }
        else if (Type == NotificationType.Push)
        {
            if (string.IsNullOrWhiteSpace(Value) || Value.Length < 10)
            {
                yield return new ValidationResult("Invalid push notification.", [nameof(Value)]);
            }
        }
        else
        {
            System.Console.WriteLine($"Invalid type: {Type}");
            yield return new ValidationResult("Invalid type.", [nameof(Value)]);
        }
    }

    [Required(ErrorMessage = "Type is required.")]
    [EnumDataType(typeof(NotificationType), ErrorMessage = "Invalid notification type.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "Value is required.")]
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
}

public class UpdateNotificationRecipientRequestDTO
{
    [Required(ErrorMessage = "Value is required.")]
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool? IsDeleted { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
}

public class QueryNotificationRecipientsRequestDTO : PaginationQueryParams
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("type")]
    public NotificationType? Type { get; set; }

    [JsonPropertyName("search")]
    public string? Search { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool? IsDeleted { get; set; }
}


public record NotificationRecipientResponseDTO : BaseEntityResponse
{

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("type")]
    public NotificationType Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "No Name";

}
