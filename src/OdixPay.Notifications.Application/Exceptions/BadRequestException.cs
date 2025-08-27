
using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class BadRequestException(string message = "Bad request", Dictionary<string, string[]>? errors = null) : AppException(message, AppStatusCodes.BadRequest, errors)
{
}