using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;

namespace OdixPay.Notifications.API.Controllers;

public abstract class BaseController : ControllerBase
{
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

    public BaseController(IStringLocalizer<SharedResource> IStringLocalizer)
    {
        _IStringLocalizer = IStringLocalizer;
    }


    // Helper methods for common responses
    protected IActionResult SuccessResponse<T>(T data, string? message = null)
    {
        var response = StandardResponse<T, object>.Success(data, message ?? _IStringLocalizer["OperationCompletedSuccessfully"]);
        return Ok(response);
    }

    protected IActionResult CreatedResponse<T>(T data)
    {
        var response = StandardResponse<T, object>.Success(data, _IStringLocalizer["ResourceCreatedSuccessfully"]);
        return Created(string.Empty, response);
    }

    protected IActionResult NotFoundResponse(string? message = null)
    {
        var response = StandardResponse<object, object>.Error(null, message ?? _IStringLocalizer["ResourceNotFound"]);
        return NotFound(response);
    }

    protected IActionResult BadRequestResponse(string? message = null)
    {
        var response = StandardResponse<object, object>.Error(null, message ?? _IStringLocalizer["BadRequest"]);
        return BadRequest(response);
    }

    protected IActionResult UnauthorizedResponse(string? message = null)
    {
        var response = StandardResponse<object, object>.Error(null, message ?? _IStringLocalizer["Unauthorized"]);
        return Unauthorized(response);
    }
}