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
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;

namespace OdixPay.Notifications.API.Controllers;

[Route($"{ApiConstants.APIVersion.VersionRouteName}/notifications")]
[ApiController]
[ApiVersion(ApiConstants.APIVersion.VersionString)]
[Authorize(AuthenticationSchemes = ApiConstants.Authentication.CustomAuthScheme)]
public class NotificationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

    public NotificationsController(IMediator mediator,
                                    IStringLocalizer<SharedResource> IStringLocalizer) : base(IStringLocalizer)
    {
        _mediator = mediator;
        _IStringLocalizer = IStringLocalizer;
    }

    [HttpPost]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Create)]
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
    public async Task<IActionResult> GetUserNotifications(
        string userId,
        [FromQuery] QueryNotifications query)
    {
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError(_IStringLocalizer["UserIdIsRequired"]));

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
    public async Task<IActionResult> GetUserNotifications(
        [FromQuery] QueryNotifications query)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException(_IStringLocalizer["UserIDNotFoundInClaims"]);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError(_IStringLocalizer["UserIdIsRequired"]));

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
    public async Task<IActionResult> GetUserUnreadCount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException(_IStringLocalizer["UserIDNotFoundInClaims"]);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(StandardResponse<object, object>.ValidationError(_IStringLocalizer["UserIdIsRequired"]));

        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        return SuccessResponse(result);
    }

    [HttpGet("user/{userId}/unread-count")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Read)]
    public async Task<IActionResult> GetUnreadCount(string userId)
    {
        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        return SuccessResponse(result);
    }

    [HttpPost("{id}/send")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Update)]
    public async Task<IActionResult> SendNotification(Guid id)
    {
        var command = new SendNotificationCommand(id);
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }

    [HttpPost("{id}/mark-read")]
    [AuthorizeRoleFilter(Permission = Permissions.Notification.Update)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var result = await _mediator.Send(command);
        return SuccessResponse(result);
    }
}
