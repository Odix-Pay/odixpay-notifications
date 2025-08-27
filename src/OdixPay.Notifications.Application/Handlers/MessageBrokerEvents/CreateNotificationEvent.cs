using MediatR;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers.MessageBrokerEvents;

public class NotificationCreatedCommandHandler(IMediator mediator, ILogger<NotificationCreatedCommandHandler> logger) : INotificationCommandHandler
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly ILogger<NotificationCreatedCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<NotificationResponse> HandleCreateNotificationAsync(CreateNotificationRequest request)
    {
        _logger.LogInformation("Handling CreateNotificationRequest for UserId: {UserId}", request.UserId);
        if (request == null)
        {
            _logger.LogError("CreateNotificationRequest is null.");
            throw new ArgumentNullException(nameof(request), "CreateNotificationRequest cannot be null.");
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
            throw new BadRequestException("NotificationId cannot be empty.");
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