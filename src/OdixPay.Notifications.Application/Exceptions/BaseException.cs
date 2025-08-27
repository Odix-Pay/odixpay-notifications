using OdixPay.Notifications.Application.Constants;

namespace OdixPay.Notifications.Application.Exceptions;

public class AppException(
    string message,
    int statusCode = AppStatusCodes.InternalServerError,
    // string? errorCode = null,
    object? errorData = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    // public string? ErrorCode { get; } = errorCode;
    public object? ErrorData { get; } = errorData;
}