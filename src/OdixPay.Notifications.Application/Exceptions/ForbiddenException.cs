using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class ForbiddenException(string message = "Forbidden") : AppException(message, AppStatusCodes.Forbidden)
{
}