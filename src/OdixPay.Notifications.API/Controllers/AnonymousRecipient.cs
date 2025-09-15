using MediatR;
using Microsoft.AspNetCore.Mvc;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Contracts.Constants;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace OdixPay.Notifications.API.Controllers;


public class CreateAnonymousRecipientRequestDTO : CreateNotificationRecipientRequestDTO
{
    [JsonPropertyName("userIds")]
    [Required(ErrorMessage = "UserIds is required")]
    [MinLength(1, ErrorMessage = "At least one UserId must be provided")]
    public List<string> UserIds { get; set; }
}

[Route($"{APIConstants.APIVersion.VersionRouteName}/notifications-recipients")]
[ApiController]
[ApiVersion(APIConstants.APIVersion.VersionString)]
public class AnonymousNotificationRecipientController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("anonymous")]
    public async Task<IActionResult> CreateAnonymousNotificationRecipient([FromBody] CreateAnonymousRecipientRequestDTO request)
    {
        if (request.UserIds == null || request.UserIds.Count == 0)
        {
            return BadRequest("UserIds cannot be null or empty.");
        }

        var promises = new List<Task<NotificationRecipientResponseDTO>>();
        foreach (var userId in request.UserIds)
        {
            if (string.IsNullOrWhiteSpace(userId))
                continue;

            var command = new CreateNotificationRecipientCommand()
            {
                Type = request.Type,
                UserId = userId,
                Value = request.Value,
                Name = request.Name
            };

            promises.Add(_mediator.Send(command));
        }

        var result = await Task.WhenAll(promises);

        return CreatedResponse(result);
    }
}