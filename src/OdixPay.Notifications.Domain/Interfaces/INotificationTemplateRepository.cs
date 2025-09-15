using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplate>> GetTemplatesByNameOrSlugAsync(string nameOrSlug, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetTemplateBySlugAndLocaleAsync(string slug, string locale, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplate>> GetActiveTemplatesAsync(int Page, int Limit, CancellationToken cancellationToken = default);
    Task UpdateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetTemplatesCountAsync(GetTemplatesQueryDTO query, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(GetTemplatesQueryDTO query, CancellationToken cancellationToken = default);
}