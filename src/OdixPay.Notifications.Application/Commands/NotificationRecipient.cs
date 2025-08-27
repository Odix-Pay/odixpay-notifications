using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;

namespace OdixPay.Notifications.Application.Commands;

public class CreateNotificationRecipientCommand : CreateNotificationRecipientRequestDTO, IRequest<NotificationRecipientResponseDTO>
{
    [Required(ErrorMessage = "UserId is required.")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
}

public class UpdateNotificationRecipientCommand : UpdateNotificationRecipientRequestDTO, IRequest<NotificationRecipientResponseDTO>
{
    [Required(ErrorMessage = "UserId is required.")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [Required(ErrorMessage = "Id is required.")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}

public class QueryNotificationRecipientsCommand : QueryNotificationRecipientsRequestDTO, IRequest<PaginatedResponseDTO<NotificationRecipientResponseDTO>>
{
}

public class GetNotificationRecipientCommand : IRequest<NotificationRecipientResponseDTO>
{
    [Required(ErrorMessage = "Id is required.")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}

public class DeleteNotificationRecipientCommand : IRequest<bool>
{

    [Required(ErrorMessage = "Id is required.")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}

