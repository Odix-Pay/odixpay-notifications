using MediatR;
using OdixPay.Notifications.Domain.Constants;
using OdixPay.Notifications.Domain.DTO.Requests;

namespace OdixPay.Notifications.Application.Commands;

public class CreateNotificationCommand : CreateNotificationRequest, IRequest<NotificationResponse>
{
}

public class SendNotificationCommand(Guid notificationId, string locale = NotificationConstants.DefaultLocale) : IRequest<bool>
{
    public Guid NotificationId { get; set; } = notificationId;
    public string Locale { get; set; } = locale;
}

public class MarkNotificationAsReadCommand(Guid notificationId) : IRequest<bool>
{
    public Guid NotificationId { get; set; } = notificationId;
}

public class MarkAllNotificationsAsReadCommand(string userIdOrRecipientId) : IRequest<bool>
{
    public string UserIdOrRecipientId { get; set; } = userIdOrRecipientId;
}

public class CreateTemplateCommand : CreateTemplateRequest, IRequest<TemplateResponse>
{
}

public class UpdateTemplateCommand(Guid id, UpdateTemplateRequest request) : IRequest<TemplateResponse>
{
    public Guid Id { get; set; } = id;
    public UpdateTemplateRequest Request { get; set; } = request;
}

public class DeleteTemplateCommand(Guid id) : IRequest<bool>
{
    public Guid Id { get; set; } = id;
}
