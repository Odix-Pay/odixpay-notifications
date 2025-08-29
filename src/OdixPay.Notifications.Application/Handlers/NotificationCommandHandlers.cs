using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Application.UseCases;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Utils;

namespace OdixPay.Notifications.Application.Handlers;

public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, NotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ValidateTemplateVariables _validateTemplateVariables;
    private readonly ILogger<CreateNotificationHandler> _logger;
    private readonly INotificationRecipientRepository _notificationRecipientRepo;
    private readonly ITemplateEngine _templateEngine;
    private readonly INotificationTemplateRepository _notificationTemplateRepository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;


    public CreateNotificationHandler(
        INotificationRepository notificationRepository,
        ValidateTemplateVariables validateTemplateVariables,
        ILogger<CreateNotificationHandler> logger,
        INotificationRecipientRepository notificationRecipientRepository,
        ITemplateEngine templateEngine,
        INotificationTemplateRepository notificationTemplateRepository,
        IMapper mapper,
        IStringLocalizer<SharedResource> StringLocalizer)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _validateTemplateVariables = validateTemplateVariables ?? throw new ArgumentNullException(nameof(validateTemplateVariables));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationRecipientRepo = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));
            _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
            _notificationTemplateRepository = notificationTemplateRepository ?? throw new ArgumentNullException(nameof(notificationTemplateRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _IStringLocalizer = StringLocalizer;
        }


    public async Task<NotificationResponse> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateNotificationCommand for UserId: {UserId}", JsonSerializer.Serialize(request));


        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title ?? string.Empty,
            Message = request.Message ?? string.Empty,
            Status = NotificationStatus.Pending,
            Priority = request.Priority,
            Recipient = request.Recipient,
            ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
            RetryCount = 0,
            MaxRetries = request.MaxRetries
        };

        if (request.Type != null)
        {
            notification.Type = request.Type.Value;
        }
        else
        {
            // Default to InApp if no type is specified
            notification.Type = NotificationType.InApp;
        }

        if (request.Data != null)
        {
            notification.Data = JsonSerializer.Serialize(request.Data);
        }

        // if userId is provided, and there is no recipient, get the user token
        if (!string.IsNullOrWhiteSpace(request.UserId) && string.IsNullOrWhiteSpace(request.Recipient))
        {
            var recipient = await _notificationRecipientRepo.GetByUserIdAndTypeAsync(request.UserId, notification.Type, cancellationToken) ?? throw new NotFoundException($"No recipient found for user and notification type: {request.UserId}, {notification.Type}");

            _logger.LogInformation("Found recipient {RecipientId} for user {UserId}", recipient?.Id, request.UserId);

            notification.Recipient = recipient!.Recipient;
        }

        // If a template is provided, validate and set it
        if ((request.TemplateId.HasValue || !string.IsNullOrWhiteSpace(request.TemplateSlug)) && request.TemplateVariables != null)
        {
            try
            {
                NotificationTemplate? template = null;

                if (request.TemplateId.HasValue)
                {
                    template = await _notificationTemplateRepository.GetTemplateByIdAsync(request.TemplateId.Value, cancellationToken);
                }
                else if (!string.IsNullOrWhiteSpace(request.TemplateSlug))
                {
                    template = await _notificationTemplateRepository.GetTemplateByNameAsync(request.TemplateSlug, cancellationToken);
                }

                if (template == null)
                {
                   
                    throw new NotFoundException(_IStringLocalizer["NotificationTemplateNotFound"]?.Value);
                }

                _logger.LogInformation("Using template {TemplateId} for notification", template.Id);

                // Validate receipient and template variables
                switch (template.Type)
                {
                    case NotificationType.Email:
                        ValidateEmailRecipient(request.Recipient);
                        break;
                    case NotificationType.SMS:
                        ValidateSmsRecipient(request.Recipient);
                        break;
                    case NotificationType.Push:
                        ValidatePushRecipient(request.Recipient);
                        break;
                    case NotificationType.InApp:
                        // In-app notifications do not require a recipient
                        break;
                }


                // Validate the template variables against the template
                if (!await _validateTemplateVariables.ExecuteAsync(new ValidateTemplateVariablesDTO
                {
                    TemplateId = template.Id,
                    Variables = request.TemplateVariables
                }, cancellationToken))
                {
                    throw new BadRequestException(_IStringLocalizer["TemplateVariablesDoNotMatch"]?.Value);
                }

                _logger.LogInformation("Template variables validated successfully for template {TemplateId}", request.TemplateId);

                // Set Overrides
                notification.Type = template.Type; // override type from template

                // 3. Use TemplateEngine here!
                notification.Title = _templateEngine.ProcessTemplate(template.Subject, request.TemplateVariables, cancellationToken);

                notification.Message = _templateEngine.ProcessTemplate(template.Body, request.TemplateVariables, cancellationToken);
                notification.TemplateId = template.Id;

                notification.TemplateVariables = JsonSerializer.Serialize(request.TemplateVariables);
            }
            catch (BadRequestException ex)
            {
                _logger.LogError(ex, "Invalid template variables provided for template {TemplateId}", request.TemplateId);
                throw; // Re-throw to be handled by the caller
            }
            catch (System.Exception ex)
            {
                // Log the error and rethrow
                _logger.LogError("Error validating template variables: {Message}", ex.Message);
                throw new BadRequestException(_IStringLocalizer["InvalidTemplateVariablesProvided"]?.Value);
            }
        }

        // if priority is High or Critical, schedule nofication for sending now
        if (request.Priority == NotificationPriority.High || request.Priority == NotificationPriority.Critical)
        {
            notification.ScheduledAt = DateTime.UtcNow; // Set to now for immediate processing. Backfround job will pick up immediately and send
        }

        var created = await _notificationRepository.CreateNotificationAsync(notification, cancellationToken);


        return _mapper.Map<NotificationResponse>(created);
    }

    private  void ValidateEmailRecipient(string? recipient)
    {
        if (!Utils.IsValidEmail(recipient))
        {
            throw new BadRequestException(_IStringLocalizer["InvalidEmailRecipientProvided"]?.Value);
        }
    }

    private  void ValidatePushRecipient(string? recipient)
    {
        if (!Utils.IsValidPushNotificationToken(recipient))
        {
            throw new BadRequestException(_IStringLocalizer["InvalidPushRecipientProvided"]?.Value);
        }
    }

    private void ValidateSmsRecipient(string? recipient)
    {
        if (!Utils.IsValidPhoneNumber(recipient))
        {
            throw new BadRequestException(_IStringLocalizer["InvalidSMSRecipientProvided"]?.Value);
        }
    }
}

public class SendNotificationHandler(
    INotificationRepository notificationRepository,
    INotificationService notificationService,
    IStringLocalizer<SharedResource> stringLocalizer) : IRequestHandler<SendNotificationCommand, bool>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer = stringLocalizer;

    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(request.NotificationId);

        if (notification == null)
            return false;

        var success = await _notificationService.SendNotificationAsync(notification, cancellationToken);

        if (success.Success)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = success.SentAt ?? DateTime.UtcNow;
            notification.ExternalId = success.ExternalId ?? string.Empty;

            await _notificationRepository.UpdateNotificationSentAsync(notification.Id, notification.SentAt.Value, notification.ExternalId ?? string.Empty, cancellationToken);
        }
        else
        {
            await _notificationRepository.IncrementRetryCountAsync(notification.Id, cancellationToken);

            await _notificationRepository.UpdateNotificationStatusAsync(
                notification.Id,
                NotificationStatus.Failed,
                _IStringLocalizer["FailedToSendNotification"]?.Value, cancellationToken);
        }

        return success.Success;
    }
}

public class MarkNotificationAsReadHandler(INotificationRepository notificationRepository) : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAsReadAsync(request.NotificationId, cancellationToken);
        return true;
    }
}
