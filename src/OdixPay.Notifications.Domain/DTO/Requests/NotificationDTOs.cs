using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class CreateNotificationBaseRequest : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(UserId) && string.IsNullOrWhiteSpace(Recipient))
        {
            yield return new ValidationResult("UserId or Recipient is required.", [nameof(UserId), nameof(Recipient)]);
        }

        // If notification typem is email and receipient available, receipient must be a valid email address
        if (Type == NotificationType.Email && !string.IsNullOrWhiteSpace(Recipient))
        {
            bool isValidEmail = true;
            try
            {
                var email = new System.Net.Mail.MailAddress(Recipient);
                if (email.Address != Recipient)
                {
                    isValidEmail = false;
                }
            }
            catch
            {
                isValidEmail = false;
            }

            if (!isValidEmail)
            {
                yield return new ValidationResult("Recipient must be a valid email address.", new[] { nameof(Recipient) });
            }
        }

        // If notification type is SMS and recipient available, recipient must be a valid phone number
        if (Type == NotificationType.SMS && !string.IsNullOrWhiteSpace(Recipient))
        {
            // Simple phone number validation (can be improved with regex)
            var isValidPhoneNumber = true;
            var message = string.Empty;
            try
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var parsedNumber = phoneNumberUtil.Parse(Recipient, null);

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
                yield return new ValidationResult(message, [nameof(Recipient)]);
            }
        }

        // If notification type is Push and recipient available, recipient must be a valid device token
        if (Type == NotificationType.Push && !string.IsNullOrWhiteSpace(Recipient))
        {
            // Simple device token validation (can be improved with regex)
            if (Recipient.Length < 10)
            {
                yield return new ValidationResult("Recipient must be a valid device token.", [nameof(Recipient)]);
            }
        }

        // If notification type is InApp, UserId must be provided
        if (Type == NotificationType.InApp && string.IsNullOrWhiteSpace(UserId))
        {
            yield return new ValidationResult("UserId is required for InApp notifications.", [nameof(UserId)]);
        }

        if (TemplateId == Guid.Empty || TemplateId == null && string.IsNullOrWhiteSpace(TemplateSlug))
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                yield return new ValidationResult("Title is required.", [nameof(Title)]);
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                yield return new ValidationResult("Message is required.", [nameof(Message)]);
            }
        }
        else
        {
            System.Console.WriteLine("TemplateId is provided: " + TemplateId);
        }
    }


    [JsonPropertyName("userId")]
    public string? UserId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [EnumDataType(typeof(NotificationType), ErrorMessage = "Invalid notification type.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NotificationType? Type { get; set; }

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Data { get; set; }

    [JsonPropertyName("priority")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [EnumDataType(typeof(NotificationPriority), ErrorMessage = "Invalid notification priority.")]
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    [JsonPropertyName("recipient")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Recipient { get; set; }
    [JsonPropertyName("sender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sender { get; set; }

    [JsonPropertyName("scheduledAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ScheduledAt { get; set; }

    [JsonPropertyName("maxRetries")]
    [Range(1, 10, ErrorMessage = "Max retries must be between 1 and 10.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MaxRetries { get; set; } = 3;

    [JsonPropertyName("templateId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? TemplateId { get; set; }  // Reference to template

    [JsonPropertyName("templateSlug")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateSlug { get; set; }

    [JsonPropertyName("locale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; set; } = "en";  // Default to English
}

public class CreateNotificationRequest : CreateNotificationBaseRequest
{
    [JsonPropertyName("templateVariables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? TemplateVariables { get; set; }  // JSON with variable values for
}

public record NotificationResponse : BaseEntityResponse
{
    [JsonPropertyName("userId")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string UserId { get; set; }

    [JsonPropertyName("type")]
    public NotificationType Type { get; set; }

    [JsonPropertyName("title")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Title { get; set; }

    [JsonPropertyName("message")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Data { get; set; }

    [JsonPropertyName("status")]
    public NotificationStatus Status { get; set; }

    [JsonPropertyName("priority")]
    public NotificationPriority Priority { get; set; }

    [JsonPropertyName("recipient")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Recipient { get; set; }

    [JsonPropertyName("scheduledAt")]
    public DateTime? ScheduledAt { get; set; }

    [JsonPropertyName("sentAt")]
    public DateTime? SentAt { get; set; }

    [JsonPropertyName("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [JsonPropertyName("errorMessage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("retryCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int RetryCount { get; set; }

    [JsonPropertyName("maxRetries")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MaxRetries { get; set; }

    [JsonPropertyName("externalId")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    [JsonPropertyName("templateId")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? TemplateId { get; set; }  // Reference to template

    [JsonPropertyName("templateVariables")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateVariables { get; set; }  // JSON with variable values for

    [JsonPropertyName("isRead")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsRead { get; set; }

    [JsonPropertyName("sender")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sender { get; set; } // Optional sender field
}

public class NotificationListResponse : PaginatedResponseDTO<NotificationResponse> { }

public class CreateTemplateRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Type is required.")]
    [JsonPropertyName("type")]
    [EnumDataType(typeof(NotificationType), ErrorMessage = "Invalid notification type.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [Required(ErrorMessage = "Body is required.")]
    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TemplateVariableStructure>? Variables { get; set; }

    [JsonPropertyName("locale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; set; }
}

public class UpdateTemplateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    [EnumDataType(typeof(NotificationType), ErrorMessage = "Invalid notification type.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType? Type { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, TemplateVariableStructure>? Variables { get; set; }
}

public record TemplateResponse : BaseEntityResponse
{

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public NotificationType Type { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    public string? Variables { get; set; }
}



