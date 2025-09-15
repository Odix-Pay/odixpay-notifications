using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Queries;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.API.Filters;
using OdixPay.Notifications.Contracts.Constants;

namespace OdixPay.Notifications.API.Controllers;

[Route($"{APIConstants.APIVersion.VersionRouteName}/notification-templates")]
[ApiController]
[ApiVersion(APIConstants.APIVersion.VersionString)]
[Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
public class TemplatesController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Create)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request)
    {
        var command = new CreateTemplateCommand()
        {
            Body = request.Body,
            Name = request.Name,
            Type = request.Type,
            Subject = request.Subject,
            Variables = request.Variables
        };
        var result = await _mediator.Send(command);

        return CreatedResponse(result);
    }

    [HttpGet]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Read)]
    public async Task<IActionResult> GetTemplate([FromQuery] GetTemplatesQueryDTO request)
    {
        var query = new GetTemplatesQuery()
        {
            Page = request.Page,
            Limit = request.Limit,
            Id = request.Id,
            Type = request.Type,
            Name = request.Name,
            Search = request.Search,
            Slug = request.Slug,
            Subject = request.Subject
        };

        var result = await _mediator.Send(query);

        return SuccessResponse(result);
    }

    [HttpGet("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Read)]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var query = new GetTemplateByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return SuccessResponse(result);
    }

    [HttpGet("by-name/{name}")]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Read)]
    public async Task<IActionResult> GetTemplateByName(string name)
    {
        var query = new GetTemplateByNameQuery(name);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return SuccessResponse(result);
    }

    [HttpGet("by-slug/{slug}")]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Read)]
    public async Task<IActionResult> GetTemplateBySlug(string slug)
    {
        var query = new GetTemplateByNameQuery(slug);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Delete)]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var command = new DeleteTemplateCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFoundResponse();

        return SuccessResponse(true);
    }

    [HttpPut("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Template.Update)]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        var command = new UpdateTemplateCommand(id, request);
        var result = await _mediator.Send(command);

        if (result == null)
            return NotFoundResponse();

        return SuccessResponse(result);
    }
}
