using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<Notification?> GetNotificationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetNotificationsAsync(QueryNotifications query, CancellationToken cancellationToken = default);
    Task<int> GetNotificationsCountAsync(QueryNotifications query, CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default);
    Task UpdateNotificationStatusAsync(Guid id, NotificationStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
    Task UpdateNotificationSentAsync(Guid id, DateTime sentAt, string? externalId = null, CancellationToken cancellationToken = default);
    Task UpdateNotificationDeliveredAsync(Guid id, DateTime deliveredAt, CancellationToken cancellationToken = default);
    Task IncrementRetryCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userIdOrRecipientId, CancellationToken cancellationToken = default);
}


