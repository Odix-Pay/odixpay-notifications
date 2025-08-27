using System.Text.Json;
using AutoMapper;
using MediatR;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers;

public class CreateTemplateHandler(INotificationTemplateRepository templateRepository, IMapper mapper) : IRequestHandler<CreateTemplateCommand, TemplateResponse>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

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
            throw new BadRequestException($"Template with name '{template.Name}' already exists.");
        }

        var result = await _templateRepository.CreateTemplateAsync(template, cancellationToken);

        return _mapper.Map<TemplateResponse>(result);
    }
}

public class UpdateTemplateHandler(INotificationTemplateRepository templateRepository, IMapper mapper) : IRequestHandler<UpdateTemplateCommand, TemplateResponse>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<TemplateResponse> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var existingTemplate = await _templateRepository.GetTemplateByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException($"Template with ID '{request.Id}' not found.");

        var req = request.Request ?? throw new BadRequestException("Update request cannot be null.");

        existingTemplate.Name = req.Name ?? existingTemplate.Name;
        existingTemplate.Type = req.Type ?? existingTemplate.Type;
        existingTemplate.Subject = req.Subject ?? existingTemplate.Subject;
        existingTemplate.Body = req.Body ?? existingTemplate.Body;
        existingTemplate.Variables = JsonSerializer.Serialize(req.Variables ?? JsonSerializer.Deserialize<Dictionary<string, TemplateVariableStructure>>(existingTemplate?.Variables ?? "{}") ?? new Dictionary<string, TemplateVariableStructure>());

        // Ensure template name is unique
        if (await _templateRepository.GetTemplateByNameAsync(existingTemplate.Name, cancellationToken) != null &&
            existingTemplate.Name != req.Name)
        {
            throw new BadRequestException($"Template with name '{req.Name}' already exists.");
        }

        existingTemplate.GenerateSlug(); // Ensure slug is updated

        await _templateRepository.UpdateTemplateAsync(existingTemplate!, cancellationToken);

        return _mapper.Map<TemplateResponse>(existingTemplate);
    }
}

public class DeleteTemplateHandler(INotificationTemplateRepository templateRepository, INotificationRepository notificationRepository) : IRequestHandler<DeleteTemplateCommand, bool>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetTemplateByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException($"Template with ID '{request.Id}' not found.");

        // Ensure the template is not in use
        var notificationsInUse = await _notificationRepository.GetNotificationsCountAsync(new()
        {
            TemplateId = request.Id
        }, cancellationToken);

        if (notificationsInUse > 0)
        {
            throw new BadRequestException($"Template with ID '{request.Id}' is currently in use and cannot be deleted.");
        }

        await _templateRepository.DeleteTemplateAsync(template.Id, cancellationToken);

        return true;
    }
}
