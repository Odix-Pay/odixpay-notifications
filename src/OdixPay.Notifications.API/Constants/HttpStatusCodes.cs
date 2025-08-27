namespace OdixPay.Notifications.API.Constants;

using Microsoft.AspNetCore.Http;


public static class HttpStatusCodes
{
    public const int Ok = StatusCodes.Status200OK;
    public const int Created = StatusCodes.Status201Created;
    public const int NoContent = StatusCodes.Status204NoContent;

    public const int BadRequest = StatusCodes.Status400BadRequest;
    public const int Unauthorized = StatusCodes.Status401Unauthorized;
    public const int Forbidden = StatusCodes.Status403Forbidden;
    public const int NotFound = StatusCodes.Status404NotFound;
    public const int UnprocessableEntity = StatusCodes.Status422UnprocessableEntity;

    public const int InternalServerError = StatusCodes.Status500InternalServerError;
}
