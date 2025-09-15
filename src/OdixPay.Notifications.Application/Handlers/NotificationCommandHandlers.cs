using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Application.UseCases;
using OdixPay.Notifications.Domain.Constants;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Utils;

namespace OdixPay.Notifications.Application.Handlers;

public class CreateNotificationHandler(INotificationRepository notificationRepository, ValidateTemplateVariables validateTemplateVariables, ILogger<CreateNotificationHandler> logger, INotificationRecipientRepository notificationRecipientRepository,
// ITemplateEngine templateEngine,
INotificationTemplateRepository notificationTemplateRepository, IMapper mapper) : IRequestHandler<CreateNotificationCommand, NotificationResponse>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly ValidateTemplateVariables _validateTemplateVariables = validateTemplateVariables ?? throw new ArgumentNullException(nameof(validateTemplateVariables));
    private readonly ILogger<CreateNotificationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly INotificationRecipientRepository _notificationRecipientRepo = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));

    // private readonly ITemplateEngine _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
    private readonly INotificationTemplateRepository _notificationTemplateRepository = notificationTemplateRepository ?? throw new ArgumentNullException(nameof(notificationTemplateRepository));

    public async Task<NotificationResponse> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var defaultLocale = NotificationConstants.DefaultLocale;
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
            MaxRetries = request.MaxRetries,
            DefaultLocale = request.Locale ?? defaultLocale
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
        if (request.TemplateId.HasValue || !string.IsNullOrWhiteSpace(request.TemplateSlug))
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
                    var foundTemplates = await _notificationTemplateRepository.GetTemplatesByNameOrSlugAsync(request.TemplateSlug, cancellationToken);
                    template = foundTemplates.FirstOrDefault();
                }

                if (template == null)
                {
                    throw new NotFoundException("Notification template not found.");
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
                    throw new BadRequestException("Template variables do not match the required format or are missing required fields.");
                }

                _logger.LogInformation("Template variables validated successfully for template {TemplateId}", request.TemplateId);

                // Set Overrides
                notification.Type = template.Type;   // override type from template

                // 3. Use TemplateEngine here!
                // We want to compile the message at real time when user requests for notifications.
                // We will use template engine in get notification handler to compile the message in real time and in send notification handler to compile the message before sending


                // notification.Title = _templateEngine.ProcessTemplate(template.Subject, request.TemplateVariables ?? [], cancellationToken);

                // notification.Message = _templateEngine.ProcessTemplate(template.Body, request.TemplateVariables ?? [], cancellationToken);

                // Set the template details
                // Instead of mapping with the template id, we map with the slug, so that we can easily use the right language during rendering (compilation). Fetching by slug will return all related templates for different languages
                // This will also make it easier to switch templates if needed in the future
                notification.TemplateSlug = template.Slug;
                // notification.TemplateId = template.Id;
                notification.DefaultLocale = request.Locale ?? defaultLocale;

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
                throw new BadRequestException("Invalid template variables provided.");
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

    private static void ValidateEmailRecipient(string? recipient)
    {
        if (!Utils.IsValidEmail(recipient))
        {
            throw new BadRequestException("Invalid email recipient provided.");
        }
    }

    private static void ValidatePushRecipient(string? recipient)
    {
        if (!Utils.IsValidPushNotificationToken(recipient))
        {
            throw new BadRequestException("Invalid push recipient provided.");
        }
    }

    private static void ValidateSmsRecipient(string? recipient)
    {
        if (!Utils.IsValidPhoneNumber(recipient))
        {
            throw new BadRequestException("Invalid SMS recipient provided.");
        }
    }
}

public class SendNotificationHandler(
    INotificationRepository notificationRepository,
    INotificationTemplateRepository notificationTemplateRepository,
    ITemplateEngine templateEngine,
    INotificationService notificationService) : IRequestHandler<SendNotificationCommand, bool>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly INotificationTemplateRepository _notificationTemplateRepository = notificationTemplateRepository ?? throw new ArgumentNullException(nameof(notificationTemplateRepository));
    private readonly ITemplateEngine _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));

    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(request.NotificationId, cancellationToken);

        if (notification == null)
            return false;

        System.Console.WriteLine("Sending notification: " + JsonSerializer.Serialize(notification));


        // Compile message if there is a template associated
        if (notification.TemplateId.HasValue || (!string.IsNullOrWhiteSpace(notification.TemplateSlug) && !string.IsNullOrEmpty(notification.TemplateSlug)))
        {
            System.Console.WriteLine("Compiling message for notification with template: " + notification.TemplateId + " or slug: " + notification.TemplateSlug);

            IEnumerable<NotificationTemplate> templates = [];

            if (!string.IsNullOrWhiteSpace(notification.TemplateSlug))
            {
                templates = await _notificationTemplateRepository.GetTemplatesByNameOrSlugAsync(notification.TemplateSlug, cancellationToken);
            }
            else if (notification.TemplateId.HasValue)
            {
                var foundTemplate = await _notificationTemplateRepository.GetTemplateByIdAsync(notification.TemplateId.Value, cancellationToken);
                if (foundTemplate != null)
                {
                    templates = [foundTemplate];
                }
            }

            if (!templates.Any())
            {
                throw new NotFoundException("Notification template not found for rendering.");
            }

            // Try to get the template in the requested locale
            var template = templates.FirstOrDefault(t => t.Locale.Equals(request.Locale, StringComparison.OrdinalIgnoreCase));

            // If not found, try to get the template in the notification's default locale
            template ??= templates.FirstOrDefault(t => t.Locale.Equals(notification.DefaultLocale, StringComparison.OrdinalIgnoreCase));

            // If not found, fallback to default locale
            template ??= templates.FirstOrDefault(t => t.Locale.Equals(NotificationConstants.DefaultLocale, StringComparison.OrdinalIgnoreCase));

            // If still not found, use the first available template
            template ??= templates.First();

            if (template == null)
            {
                throw new NotFoundException("Notification template not found for rendering.");
            }

            // Compile the title and message using the template engine
            var templateVariables = string.IsNullOrWhiteSpace(notification.TemplateVariables)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, string>>(notification.TemplateVariables) ?? [];

            notification.Title = _templateEngine.ProcessTemplate(template.Subject, templateVariables, cancellationToken);
            notification.Message = _templateEngine.ProcessTemplate(template.Body, templateVariables, cancellationToken);

        }

        System.Console.WriteLine("Compiled notification: " + JsonSerializer.Serialize(notification));

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
                "Failed to send notification", cancellationToken);
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

public class MarkAllNotificationsAsReadHandler(INotificationRepository notificationRepository) : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAllAsReadAsync(request.UserIdOrRecipientId, cancellationToken);
        return true;
    }
}
