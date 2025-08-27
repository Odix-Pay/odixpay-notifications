using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface INotificationService
{
    Task<SendNotificationResult> SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<SendNotificationResult> SendEmailAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<SendNotificationResult> SendSmsAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<SendNotificationResult> SendPushNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task<SendNotificationResult> SendEmailAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
}

public interface ISmsService
{
    Task<SendNotificationResult> SendSmsAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
}

public interface IPushNotificationService
{
    Task<SendNotificationResult> SendPushNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
}

public interface ITemplateEngine
{
    string ProcessTemplate(string template, Dictionary<string, string> variables, CancellationToken cancellationToken = default);
}
