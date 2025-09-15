using MediatR;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;

namespace OdixPay.Notifications.Application.Queries;

public class GetTemplatesQuery : GetTemplatesQueryDTO, IRequest<PaginatedResponseDTO<TemplateResponse>>
{
}

public class GetUnreadCountQuery : IRequest<int>
{
    public string UserId { get; set; }

    public GetUnreadCountQuery(string userId)
    {
        UserId = userId;
    }
}

public class GetTemplateByIdQuery : IRequest<TemplateResponse?>
{
    public Guid Id { get; set; }

    public GetTemplateByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetTemplateByNameQuery(string name, string? locale = null) : IRequest<TemplateResponse?>
{
    public string Name { get; set; } = name;
    public string? Locale { get; set; } = locale;
}