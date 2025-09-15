using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface INotificationRecipientRepository
{
    Task<NotificationRecipient> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<NotificationRecipient> recipients, int TotalCount)> GetByUserIdAsync(string userId, int page = 1, int limit = 20, CancellationToken cancellationToken = default);

    Task<NotificationRecipient?> GetByUserIdAndTypeAsync(string userId, NotificationType type, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default);

    Task UpdateAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default);
    Task UpdateRecipientLanguageAsync(string recipientId, string language, CancellationToken cancellationToken = default);

    Task<(IEnumerable<NotificationRecipient> Recipients, int TotalCount)> QueryAsync(QueryNotificationRecipientsRequestDTO query, CancellationToken cancellationToken = default);

    Task<int> CountAsync(QueryNotificationRecipientsRequestDTO query, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}