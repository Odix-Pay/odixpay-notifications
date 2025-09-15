using MediatR;
using OdixPay.Notifications.Domain.Constants;
using OdixPay.Notifications.Domain.DTO.Requests;

namespace OdixPay.Notifications.Application.Queries;

public class GetNotificationByIdQuery(Guid id, string? locale = null) : IRequest<NotificationResponse?>
{
    public Guid Id { get; set; } = id;
    public string? Locale { get; set; } = locale;
}

public class GetNotificationsQuery : QueryNotifications, IRequest<NotificationListResponse>
{

}


