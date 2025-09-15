using AutoMapper;
using MediatR;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Contracts.Constants;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers;

public class CreateNotificationRecipientHandler(INotificationRecipientRepository notificationReceipientRepository, IMapper mapper, IRealtimeNotifier realtimeNotifier) : IRequestHandler<CreateNotificationRecipientCommand, NotificationRecipientResponseDTO>
{

    private readonly INotificationRecipientRepository _notificationReceipientRepository = notificationReceipientRepository ?? throw new ArgumentNullException(nameof(notificationReceipientRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IRealtimeNotifier _realtimeNotifier = realtimeNotifier ?? throw new ArgumentNullException(nameof(realtimeNotifier));

    public async Task<NotificationRecipientResponseDTO> Handle(CreateNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new BadRequestException("UserId cannot be null or empty.");
        }

        // Ensure receipient does not already exist with this type.
        var existingReceipient = await _notificationReceipientRepository.GetByUserIdAndTypeAsync(request.UserId, request.Type, cancellationToken);

        // If already exists, then do an update instead
        if (existingReceipient != null)
        {
            existingReceipient.Recipient = request.Value;
            await _notificationReceipientRepository.UpdateAsync(existingReceipient, cancellationToken);
            return _mapper.Map<NotificationRecipientResponseDTO>(existingReceipient);
        }

        System.Console.WriteLine($"Creating new notification recipient for user {request.UserId} of type {request.Type}");

        var notificationReceipient = new NotificationRecipient
        {
            UserId = request.UserId,
            Type = request.Type,
            Recipient = request.Value,
            Name = request.Name
        };

        await _notificationReceipientRepository.AddAsync(notificationReceipient, cancellationToken);

        var receipient = _mapper.Map<NotificationRecipientResponseDTO>(notificationReceipient);

        await _realtimeNotifier.SendToGroupAsync(HubPrefixes.GetGroup(HubPrefixes.Admins, [HubPrefixes.Notifications]), "Notification_Recipient_Created", receipient, cancellationToken);

        return receipient;
    }
}


public class UpdateNotificationRecipientHandler(INotificationRecipientRepository notificationReceipientRepository, IMapper mapper) : IRequestHandler<UpdateNotificationRecipientCommand, NotificationRecipientResponseDTO>
{

    private readonly INotificationRecipientRepository _notificationReceipientRepository = notificationReceipientRepository ?? throw new ArgumentNullException(nameof(notificationReceipientRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<NotificationRecipientResponseDTO> Handle(UpdateNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new BadRequestException("UserId cannot be null or empty.");
        }

        // Ensure receipient does not already exist with this type.
        var existingReceipient = await _notificationReceipientRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException("Notification recipient not found.");


        // Since We cannot change the type, validate the value based on existing type
        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            var isValid = ValidateValue(existingReceipient.Type, request.Value);

            if (!isValid)
            {
                throw new BadRequestException("Invalid value for the given type.");
            }
            existingReceipient.Recipient = request.Value;
        }

        // If updating userId, ensure the user does not already have token set for this type
        if (!string.IsNullOrEmpty(request.UserId) && existingReceipient.UserId != request.UserId)
        {
            var existingToken = await _notificationReceipientRepository.GetByUserIdAndTypeAsync(request.UserId, existingReceipient.Type, cancellationToken);

            if (existingToken != null && existingToken.Id != existingReceipient.Id)
            {
                throw new BadRequestException("User already has a token set for this type.");
            }

            existingReceipient.UserId = request.UserId;
        }

        existingReceipient.MarkAsActive(request.IsActive ?? existingReceipient.IsActive);
        existingReceipient.MarkAsDeleted(request.IsDeleted ?? existingReceipient.IsDeleted);
        existingReceipient.Name = request.Name ?? existingReceipient.Name;

        await _notificationReceipientRepository.UpdateAsync(existingReceipient, cancellationToken);

        return _mapper.Map<NotificationRecipientResponseDTO>(existingReceipient);
    }

    private bool ValidateValue(NotificationType type, string value)
    {
        if (type == NotificationType.Email)
        {
            var email = new System.Net.Mail.MailAddress(value);
            return email.Address == value; // Simple email validation
        }
        else if (type == NotificationType.SMS)
        {
            try
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var parsedNumber = phoneNumberUtil.Parse(value, null);

                return phoneNumberUtil.IsValidNumber(parsedNumber);
            }
            catch (PhoneNumbers.NumberParseException)
            {
                return false;
            }
        }
        else if (type == NotificationType.Push)
        {
            // Validate push notification token format
            return !string.IsNullOrWhiteSpace(value);
        }

        return false; // Invalid type
    }
}

public class QueryNotificationRecipientHandler(INotificationRecipientRepository notificationRecipientRepository, IMapper mapper) : IRequestHandler<QueryNotificationRecipientsCommand, PaginatedResponseDTO<NotificationRecipientResponseDTO>>
{
    private readonly INotificationRecipientRepository _notificationRecipientRepository = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<PaginatedResponseDTO<NotificationRecipientResponseDTO>> Handle(QueryNotificationRecipientsCommand request, CancellationToken cancellationToken)
    {
        var limit = request.Limit.GetValueOrDefault(20);
        var page = request.Page.GetValueOrDefault(1);

        request.Limit = limit;
        request.Page = page;

        (IEnumerable<NotificationRecipient> recipients, int totalCount) = await _notificationRecipientRepository.QueryAsync(request, cancellationToken);

        return new()
        {
            Data = _mapper.Map<IEnumerable<NotificationRecipientResponseDTO>>(recipients),
            Total = totalCount,
            Limit = limit,
            Page = page
        };
    }
}

public class GetNotificationRecipientByIdHandler(INotificationRecipientRepository notificationRecipientRepository, IMapper mapper) : IRequestHandler<GetNotificationRecipientCommand, NotificationRecipientResponseDTO>
{
    private readonly INotificationRecipientRepository _notificationRecipientRepository = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<NotificationRecipientResponseDTO> Handle(GetNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await _notificationRecipientRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException("Notification recipient not found.");

        return _mapper.Map<NotificationRecipientResponseDTO>(recipient);
    }
}

public class DeleteNotificationRecipientCommandHandler(INotificationRecipientRepository notificationRecipientRepository) : IRequestHandler<DeleteNotificationRecipientCommand, bool>
{
    private readonly INotificationRecipientRepository _notificationRecipientRepository = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));

    public async Task<bool> Handle(DeleteNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await _notificationRecipientRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException("Notification recipient not found.");

        await _notificationRecipientRepository.DeleteAsync(recipient.Id, cancellationToken);

        return true;
    }
}