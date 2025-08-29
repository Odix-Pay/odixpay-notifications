using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers;

public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, TemplateResponse>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

    public CreateTemplateHandler(INotificationTemplateRepository templateRepository,
                                 IMapper mapper,
                                IStringLocalizer<SharedResource> StringLocalizer)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _IStringLocalizer = StringLocalizer;
    }


    public async Task<TemplateResponse> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new NotificationTemplate
        {
            Name = request.Name,
            Type = request.Type,
            Subject = request.Subject,
            Body = request.Body,
            Variables = JsonSerializer.Serialize(request?.Variables ?? []),
        };

        // Ensure template name is unique
        if (await _templateRepository.GetTemplateByNameAsync(template.Name, cancellationToken) != null)
        {
            throw new BadRequestException(string.Format(_IStringLocalizer["TemplateWithNameAlreadyExists"], template.Name));
        }

        var result = await _templateRepository.CreateTemplateAsync(template, cancellationToken);

        return _mapper.Map<TemplateResponse>(result);
    }
}

public class UpdateTemplateHandler : IRequestHandler<UpdateTemplateCommand, TemplateResponse>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;


    public UpdateTemplateHandler(
        INotificationTemplateRepository templateRepository,
        IMapper mapper,
        IStringLocalizer<SharedResource> StringLocalizer)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _IStringLocalizer = StringLocalizer;
    }

    public async Task<TemplateResponse> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var existingTemplate = await _templateRepository.GetTemplateByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException(string.Format(_IStringLocalizer["TemplateWithIdNotFound"], request.Id));

        var req = request.Request ?? throw new BadRequestException(_IStringLocalizer["UserRequestCannotBeNullOrEmpty"]?.Value);

        existingTemplate.Name = req.Name ?? existingTemplate.Name;
        existingTemplate.Type = req.Type ?? existingTemplate.Type;
        existingTemplate.Subject = req.Subject ?? existingTemplate.Subject;
        existingTemplate.Body = req.Body ?? existingTemplate.Body;
        existingTemplate.Variables = JsonSerializer.Serialize(req.Variables ?? JsonSerializer.Deserialize<Dictionary<string, TemplateVariableStructure>>(existingTemplate?.Variables ?? "{}") ?? new Dictionary<string, TemplateVariableStructure>());

        // Ensure template name is unique
        if (await _templateRepository.GetTemplateByNameAsync(existingTemplate.Name, cancellationToken) != null &&
            existingTemplate.Name != req.Name)
        {
            throw new BadRequestException(string.Format(_IStringLocalizer["TemplateWithNameAlreadyExisit"], req.Name));
        }

        existingTemplate.GenerateSlug(); // Ensure slug is updated

        await _templateRepository.UpdateTemplateAsync(existingTemplate!, cancellationToken);

        return _mapper.Map<TemplateResponse>(existingTemplate);
    }
}

public class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand, bool>
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;


    public DeleteTemplateHandler(
        INotificationTemplateRepository templateRepository,
        INotificationRepository notificationRepository,
        IStringLocalizer<SharedResource> iStringLocalizer)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _IStringLocalizer = iStringLocalizer;
    }

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetTemplateByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException(string.Format(_IStringLocalizer["TemplateWithIdNotFound"], request.Id));

        // Ensure the template is not in use
        var notificationsInUse = await _notificationRepository.GetNotificationsCountAsync(new()
        {
            TemplateId = request.Id
        }, cancellationToken);

        if (notificationsInUse > 0)
        {
            
            throw new BadRequestException(string.Format(_IStringLocalizer["TemplateInUseCannotBeDeleted"], request.Id));
        }

        await _templateRepository.DeleteTemplateAsync(template.Id, cancellationToken);

        return true;
    }
}
