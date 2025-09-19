using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Application.Queries;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.API.Filters;
using OdixPay.Notifications.Contracts.Constants;

namespace OdixPay.Notifications.API.Controllers;

[Route($"{APIConstants.APIVersion.VersionRouteName}/notifications")]
[ApiController]
[ApiVersion(APIConstants.APIVersion.VersionString)]

public class NotificationsController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Create)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        var command = new CreateNotificationCommand()
        {
            Data = request.Data,
            Message = request.Message,
            Title = request.Title,
            Type = request.Type,
            MaxRetries = request.MaxRetries,
            Priority = request.Priority,
            Recipient = request.Recipient,
            UserId = request.UserId,
            ScheduledAt = request.ScheduledAt,
            TemplateId = request.TemplateId,
            TemplateVariables = request.TemplateVariables
        };
        var result = await _mediator.Send(command);
        return CreatedResponse(result);
    }

    [HttpGet("{id}")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Read)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        var query = new GetNotificationByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return SuccessResponse(result);
    }

    [HttpGet("user/{userId}")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Read)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> GetUserNotifications(
        string userId,
        [FromQuery] QueryNotifications query)
    {
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError("User ID is required."));

        var command = new GetNotificationsQuery()
        {
            UserId = userId,
            Page = query.Page,
            Limit = query.Limit,
            Status = query.Status,
            Type = query.Type,
            Recipient = query.Recipient,
            Sender = query.Sender,
            TemplateId = query.TemplateId,
            Search = query.Search,
            Id = query.Id,
            IsRead = query.IsRead,
            Priority = query.Priority,
        };
        var result = await _mediator.Send(command);

        return SuccessResponse(result);
    }

    [HttpGet("account")]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> GetUserNotifications(
        [FromQuery] QueryNotifications query)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims.");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError("User ID is required."));

        var command = new GetNotificationsQuery()
        {
            UserId = userId,
            Page = query.Page,
            Limit = query.Limit,
            Status = query.Status,
            Type = query.Type,
            Recipient = query.Recipient,
            Sender = query.Sender,
            TemplateId = query.TemplateId,
            Search = query.Search,
            Id = query.Id,
            IsRead = query.IsRead,
            Priority = query.Priority,
        };
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }

    [HttpGet("account/unread-count")]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> GetUserUnreadCount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims.");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError("User ID is required."));

        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        return SuccessResponse(result);
    }

    [HttpGet("user/{userId}/unread-count")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Read)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> GetUnreadCount(string userId)
    {
        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        return SuccessResponse(result);
    }

    [HttpPost("{id}/send")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Update)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> SendNotification(Guid id)
    {
        var command = new SendNotificationCommand(id);
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }

    [HttpPost("{id}/mark-read")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Update)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }

    [HttpPost("user/{userId}/mark-read")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Update)]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> MarkAllAsReadAdmin(string userId)
    {
        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }

    [HttpPost("account/mark-all-read")]
    [Authorize(AuthenticationSchemes = APIConstants.Authentication.CustomAuthScheme)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return UnauthorizedResponse();

        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }


    [HttpGet("account-anonymous")]
    public async Task<IActionResult> GetUserNotificationsAnonymous([FromQuery] QueryNotifications query)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in claims.");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError("User ID is required."));

        var command = new GetNotificationsQuery()
        {
            UserId = userId,
            Page = query.Page,
            Limit = query.Limit,
            Status = query.Status,
            Type = query.Type,
            Recipient = query.Recipient,
            Sender = query.Sender,
            TemplateId = query.TemplateId,
            Search = query.Search,
            Id = query.Id,
            IsRead = query.IsRead,
            Priority = query.Priority,
        };
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }
}
