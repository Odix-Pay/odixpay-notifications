using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.API.Filters;

namespace OdixPay.Notifications.API.Controllers;

[Route($"{ApiConstants.APIVersion.VersionRouteName}/notifications-recipients")]
[ApiController]
[ApiVersion(ApiConstants.APIVersion.VersionString)]
[Authorize(AuthenticationSchemes = ApiConstants.Authentication.CustomAuthScheme)]
public class NotificationRecipientController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateNotificationRecipient([FromBody] CreateNotificationRecipientRequestDTO request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse();
        }

        var command = new CreateNotificationRecipientCommand()
        {
            Type = request.Type,
            UserId = userId,
            Value = request.Value,
            Name = request.Name
        };

        var result = await _mediator.Send(command);

        return CreatedResponse(result);
    }

    // Only those with required permissions can these enpoints below

    [HttpGet]
    [AuthorizeRoleFilter(Permission = Permissions.Recipient.Read)]
    public async Task<IActionResult> QueryNotificationRecipients([FromQuery] QueryNotificationRecipientsCommand request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse();
        }

        var result = await _mediator.Send(request);

        return SuccessResponse(result);
    }

    [HttpPost("admin")]
    [AuthorizeRoleFilter(Permission = Permissions.Recipient.Create)]
    public async Task<IActionResult> CreateNotificationRecipientAdmin([FromBody] CreateNotificationRecipientCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse();
        }

        var result = await _mediator.Send(command);

        return CreatedResponse(result);
    }

    [HttpGet("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Recipient.Read)]
    public async Task<IActionResult> GetNotificationRecipient(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse();
        }

        var query = new GetNotificationRecipientCommand()
        {
            Id = id,
        };

        var result = await _mediator.Send(query, cancellationToken);

        return SuccessResponse(result);
    }

    [HttpPut("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Recipient.Update)]
    public async Task<IActionResult> UpdateNotificationRecipient(Guid id, [FromBody] UpdateNotificationRecipientRequestDTO request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse();
        }

        var command = new UpdateNotificationRecipientCommand()
        {
            Id = id,
            UserId = userId,
            Value = request.Value,
            Name = request.Name,
            IsActive = request.IsActive,
            IsDeleted = request.IsDeleted
        };

        var result = await _mediator.Send(command);

        return SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Recipient.Delete)]
    public async Task<IActionResult> DeleteNotificationRecipient(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return UnauthorizedResponse();
        }

        var command = new DeleteNotificationRecipientCommand()
        {
            Id = id
        };

        var result = await _mediator.Send(command);

        return SuccessResponse(result);
    }

}