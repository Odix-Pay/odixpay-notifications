using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class UnauthorizedException(string message = "Unauthorized") : AppException(message, AppStatusCodes.Unauthorized)
{
}