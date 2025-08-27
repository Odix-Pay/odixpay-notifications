using MediatR;
using OdixPay.Notifications.Domain.DTO.Requests;

namespace OdixPay.Notifications.Application.Queries;

public class GetNotificationByIdQuery(Guid id) : IRequest<NotificationResponse?>
{
    public Guid Id { get; set; } = id;
}

public class GetNotificationsQuery : QueryNotifications, IRequest<NotificationListResponse>
{

}


