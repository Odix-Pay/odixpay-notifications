using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.Application.Constants;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;


namespace OdixPay.Notifications.Application.Exceptions;

//public class ValidationException(string message = "Validation failed", Dictionary<string, string[]>? errors = null) : AppException(message, AppStatusCodes.UnprocessableEntity, errors)
//{
//}

public class ValidationException : System.Exception
{
    public int StatusCode { get; } = StatusCodes.Status400BadRequest;
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IStringLocalizer<SharedResource> _IStringLocalizer,
                                IDictionary<string, string[]> errors)
        : base(_IStringLocalizer["ValidationFailed"])
    {
        Errors = errors;
    }

    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}