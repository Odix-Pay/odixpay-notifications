using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class ValidationException(string message = "Validation failed", Dictionary<string, string[]>? errors = null) : AppException(message, AppStatusCodes.UnprocessableEntity, errors)
{
}