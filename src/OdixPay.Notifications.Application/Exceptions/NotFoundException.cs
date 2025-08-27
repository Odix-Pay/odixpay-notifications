using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class NotFoundException(string message = "Resource not found") : AppException(message, AppStatusCodes.NotFound)
{
}