using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface INotificationCommandHandler
{
    Task<NotificationResponse> HandleCreateNotificationAsync(CreateNotificationRequest request);
    Task<bool> HandleSendNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
}