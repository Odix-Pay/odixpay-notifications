using System.Text.Json;
using AutoMapper;
using MediatR;
using OdixPay.Notifications.Application.Queries;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers;

public class GetTemplateByIdHandler(INotificationTemplateRepository templateRepository, IMapper mapper) : IRequestHandler<GetTemplateByIdQuery, TemplateResponse?>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<TemplateResponse?> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetTemplateByIdAsync(request.Id);
        if (template == null)
            return null;

        return _mapper.Map<TemplateResponse>(template);
    }
}

public class GetTemplateByNameHandler(INotificationTemplateRepository templateRepository, IMapper mapper) : IRequestHandler<GetTemplateByNameQuery, TemplateResponse?>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<TemplateResponse?> Handle(GetTemplateByNameQuery request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetTemplateByNameAsync(request.Name, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (template == null)
            return null;

        return _mapper.Map<TemplateResponse>(template);
    }
}

public class GetTemplatesHandler(INotificationTemplateRepository templateRepository, IMapper mapper) : IRequestHandler<GetTemplatesQuery, PaginatedResponseDTO<TemplateResponse>>
{
    private readonly INotificationTemplateRepository _templateRepository = templateRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedResponseDTO<TemplateResponse>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var Page = request.Page.GetValueOrDefault(1);
        var Limit = request.Limit.GetValueOrDefault(20);

        request.Page = Page;
        request.Limit = Limit;

        cancellationToken.ThrowIfCancellationRequested();

        System.Console.WriteLine("Fetching templates with parameters: " + JsonSerializer.Serialize(request));

        var templates = await _templateRepository.GetTemplatesAsync(request, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var totalCount = await _templateRepository.GetTemplatesCountAsync(request, cancellationToken);

        return new PaginatedResponseDTO<TemplateResponse>
        {
            Data = templates.Select(t => _mapper.Map<TemplateResponse>(t)),
            Page = Page,
            Limit = Limit,
            Total = totalCount
        };
    }
}