using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;

namespace OdixPay.Notifications.Application.Commands;

public class CreateNotificationRecipientCommand : CreateNotificationRecipientRequestDTO, IRequest<NotificationRecipientResponseDTO>
{
    [JsonPropertyName("userId")]
    [Required(ErrorMessageResourceType = typeof(SharedResource),
        ErrorMessageResourceName = "UserIdIsRequired")]
    public string UserId { get; set; }
}

public class UpdateNotificationRecipientCommand : UpdateNotificationRecipientRequestDTO, IRequest<NotificationRecipientResponseDTO>
{

    [JsonPropertyName("userId")]
    [Required(ErrorMessageResourceType = typeof(SharedResource),
        ErrorMessageResourceName = "UserIdIsRequired")]
    public string UserId { get; set; }

    [JsonPropertyName("id")]
    [Required(ErrorMessageResourceType = typeof(SharedResource),
        ErrorMessageResourceName = "IdIsRequired")]
    public Guid Id { get; set; }
}

public class QueryNotificationRecipientsCommand : QueryNotificationRecipientsRequestDTO, IRequest<PaginatedResponseDTO<NotificationRecipientResponseDTO>>
{
}

public class GetNotificationRecipientCommand : IRequest<NotificationRecipientResponseDTO>
{
    [JsonPropertyName("id")]
    [Required(
        ErrorMessageResourceType = typeof(SharedResource),
        ErrorMessageResourceName = "IdIsRequired")]
    public Guid Id { get; set; }
}

public class DeleteNotificationRecipientCommand : IRequest<bool>
{
    [JsonPropertyName("id")]
    [Required(ErrorMessageResourceType = typeof(SharedResource),
        ErrorMessageResourceName = "IdIsRequired")]
    public Guid Id { get; set; }
}

