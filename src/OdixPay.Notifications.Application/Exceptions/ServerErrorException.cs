using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class ServerErrorException(string message = "Unexpected server error") : AppException(message, AppStatusCodes.InternalServerError)
{
}