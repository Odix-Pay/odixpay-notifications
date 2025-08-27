using Microsoft.AspNetCore.Mvc;
using OdixPay.Notifications.API.Models.Response;

namespace OdixPay.Notifications.API.Controllers;

public abstract class BaseController : ControllerBase
{

    // Helper methods for common responses
    protected IActionResult SuccessResponse<T>(T data, string? message = null)
    {
        var response = StandardResponse<T, object>.Success(data, message);
        return Ok(response);
    }

    protected IActionResult CreatedResponse<T>(T data)
    {
        var response = StandardResponse<T, object>.Success(data, "Resource created successfully.");
        return Created(string.Empty, response);
    }

    protected IActionResult NotFoundResponse(string? message = null)
    {
        var response = StandardResponse<object, object>.Error(null, message ?? "Resource not found.");
        return NotFound(response);
    }

    protected IActionResult BadRequestResponse(string? message = null)
    {
        var response = StandardResponse<object, object>.Error(null, message ?? "Bad request.");
        return BadRequest(response);
    }

    protected IActionResult UnauthorizedResponse(string? message = null)
    {
        var response = StandardResponse<object, object>.Error(null, message ?? "Unauthorized.");
        return Unauthorized(response);
    }
}