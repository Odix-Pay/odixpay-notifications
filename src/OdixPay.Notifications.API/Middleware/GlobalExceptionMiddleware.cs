using OdixPay.Notifications.Contracts.Constants;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Application.Exceptions;

namespace OdixPay.Notifications.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentException(nameof(next));
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger ?? throw new ArgumentException(nameof(logger));

    public async Task InvokeAsync(HttpContext context)
    {

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int statusCode = HttpStatusCodes.InternalServerError;
        object? errorPayload = null;
        string message = "An unexpected error occurred";

        if (ex is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            errorPayload = appEx.ErrorData;
            message = appEx.Message;
        }

        var response = new StandardResponse<object, object>(
            status: APIConstants.Response.ErrorStatus,
            data: null,
            error: errorPayload,
            message: message
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);

    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}