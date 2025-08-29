using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers;

public class CreateNotificationRecipientHandler : IRequestHandler<CreateNotificationRecipientCommand, NotificationRecipientResponseDTO>
{
    private readonly INotificationRecipientRepository _notificationRecipientRepository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;


    public CreateNotificationRecipientHandler(
        INotificationRecipientRepository notificationRecipientRepository,
        IMapper mapper,
        IStringLocalizer<SharedResource> StringLocalizer)
    {
        _notificationRecipientRepository = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _IStringLocalizer = StringLocalizer;
    }

    public async Task<NotificationRecipientResponseDTO> Handle(CreateNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new BadRequestException(_IStringLocalizer["UserIdCannotBeNullOrEmpty"]?.Value);
        }

        // Ensure receipient does not already exist with this type.
        var existingReceipient = await _notificationRecipientRepository.GetByUserIdAndTypeAsync(request.UserId, request.Type, cancellationToken);
                                       
        // If already exists, then do an update instead
        if (existingReceipient != null)
        {
            existingReceipient.Recipient = request.Value;
            await _notificationRecipientRepository.UpdateAsync(existingReceipient, cancellationToken);
            return _mapper.Map<NotificationRecipientResponseDTO>(existingReceipient);
        }


        var notificationReceipient = new NotificationRecipient
        {
            UserId = request.UserId,
            Type = request.Type,
            Recipient = request.Value,
            Name = request.Name
        };

        await _notificationRecipientRepository.AddAsync(notificationReceipient, cancellationToken);

        return _mapper.Map<NotificationRecipientResponseDTO>(notificationReceipient);
    }
}


public class UpdateNotificationRecipientHandler(INotificationRecipientRepository notificationReceipientRepository, IMapper mapper, IStringLocalizer<SharedResource> StringLocalizer) : IRequestHandler<UpdateNotificationRecipientCommand, NotificationRecipientResponseDTO>
{
    private readonly INotificationRecipientRepository _notificationReceipientRepository = notificationReceipientRepository ?? throw new ArgumentNullException(nameof(notificationReceipientRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer = StringLocalizer ?? throw new ArgumentNullException(nameof(StringLocalizer));

    public async Task<NotificationRecipientResponseDTO> Handle(UpdateNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new BadRequestException(_IStringLocalizer["UserIdCannotBeNullOrEmpty"]?.Value);
        }

        // Ensure receipient does not already exist with this type.
        var existingReceipient = await _notificationReceipientRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException("Notification recipient not found.");

        // Since We cannot change the type, validate the value based on existing type
        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            var isValid = ValidateValue(existingReceipient.Type, request.Value);

            if (!isValid)
            {
                throw new BadRequestException(_IStringLocalizer["InvalidValueForTheGivenType"]?.Value);
            }
            existingReceipient.Recipient = request.Value;
        }

        // If updating userId, ensure the user does not already have token set for this type
        if (!string.IsNullOrEmpty(request.UserId) && existingReceipient.UserId != request.UserId)
        {
            var existingToken = await _notificationReceipientRepository.GetByUserIdAndTypeAsync(request.UserId, existingReceipient.Type, cancellationToken);

            if (existingToken != null && existingToken.Id != existingReceipient.Id)
            {
                throw new BadRequestException(_IStringLocalizer["UserAlreadyHasATokenSetForThisType"]?.Value);

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

public class GetNotificationRecipientByIdHandler : IRequestHandler<GetNotificationRecipientCommand, NotificationRecipientResponseDTO>
{
    private readonly INotificationRecipientRepository _notificationRecipientRepository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

    public GetNotificationRecipientByIdHandler(INotificationRecipientRepository notificationRecipientRepository,
                                                IMapper mapper,
                                                IStringLocalizer<SharedResource> StringLocalizer)
    {
        _notificationRecipientRepository = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _IStringLocalizer = StringLocalizer ?? throw new ArgumentNullException(nameof(StringLocalizer));
    }
    

    public async Task<NotificationRecipientResponseDTO> Handle(GetNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await _notificationRecipientRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException(_IStringLocalizer["NotificationRecipientNotFound"]?.Value);

        return _mapper.Map<NotificationRecipientResponseDTO>(recipient);
    }
}

public class DeleteNotificationRecipientCommandHandler : IRequestHandler<DeleteNotificationRecipientCommand, bool>
{
    private readonly INotificationRecipientRepository _notificationRecipientRepository;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

    public DeleteNotificationRecipientCommandHandler(INotificationRecipientRepository notificationRecipientRepository,
                                                IMapper mapper,
                                                IStringLocalizer<SharedResource> StringLocalizer)
    {

        _notificationRecipientRepository = notificationRecipientRepository ?? throw new ArgumentNullException(nameof(notificationRecipientRepository));
        _IStringLocalizer = StringLocalizer ?? throw new ArgumentNullException(nameof(StringLocalizer));
    }
    
    public async Task<bool> Handle(DeleteNotificationRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await _notificationRecipientRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException(_IStringLocalizer["NotificationRecipientNotFound"]?.Value);

        await _notificationRecipientRepository.DeleteAsync(recipient.Id, cancellationToken);

        return true;
    }
}