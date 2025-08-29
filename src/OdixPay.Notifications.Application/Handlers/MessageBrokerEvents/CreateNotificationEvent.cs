using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers.MessageBrokerEvents;

public class NotificationCreatedCommandHandler : INotificationCommandHandler
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationCreatedCommandHandler> _logger;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;


    public NotificationCreatedCommandHandler(
        IMediator mediator,
        ILogger<NotificationCreatedCommandHandler> logger,
        IStringLocalizer<SharedResource> StringLocalizer)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _IStringLocalizer = StringLocalizer;
    }

    public async Task<NotificationResponse> HandleCreateNotificationAsync(CreateNotificationRequest request)
    {
        _logger.LogInformation("Handling CreateNotificationRequest for UserId: {UserId}", request.UserId);
        if (request == null)
        {
            _logger.LogError("CreateNotificationRequest is null.");
            throw new ArgumentNullException(nameof(request),_IStringLocalizer["CreateNotificationRequestCannotBeNull"]);
        }

        var command = new CreateNotificationCommand
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Data = request.Data,
            Priority = request.Priority,
            Recipient = request.Recipient,
            ScheduledAt = request.ScheduledAt,
            MaxRetries = request.MaxRetries,
            TemplateId = request.TemplateId,
            TemplateVariables = request.TemplateVariables,
            Sender = request.Sender,
            TemplateSlug = request.TemplateSlug
        };

        return await _mediator.Send(command);
    }

    public async Task<bool> HandleSendNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SendNotificationAsync for NotificationId: {NotificationId}", notificationId);
        if (notificationId == Guid.Empty)
        {
            _logger.LogError("NotificationId is empty.");
            throw new BadRequestException(_IStringLocalizer["NotificationIdCannotBeEmpty"]);
        }

        try
        {
            var result = await _mediator.Send(new SendNotificationCommand(notificationId), cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification {NotificationId}", notificationId);
            throw;
        }
    }
}