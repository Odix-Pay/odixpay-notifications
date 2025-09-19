using MediatR;
using Microsoft.AspNetCore.Mvc;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Contracts.Constants;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using OdixPay.Notifications.Domain.DTO.Requests.Notifications;

namespace OdixPay.Notifications.API.Controllers;




[Route($"{APIConstants.APIVersion.VersionRouteName}/notifications-recipients")]
[ApiController]
[ApiVersion(APIConstants.APIVersion.VersionString)]
public class AnonymousNotificationRecipientController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("anonymous")]
    public async Task<IActionResult> CreateAnonymousNotificationRecipient([FromBody] CreateAnonymousRecipientRequestDTO request)
    {
        if (request.UserId == null)
        {
            return BadRequest("UserIds cannot be null or empty.");
        }

        var promises = new List<Task<NotificationRecipientResponseDTO>>();
        

            var command = new CreateNotificationRecipientCommand()
            {
                Type = request.Type,
                UserId = request.UserId,
                Value = request.Value,
                Name = request.Name
            };
        promises.Add(_mediator.Send(command));
        var result = await Task.WhenAll(promises);

        return CreatedResponse(result);
    }
}