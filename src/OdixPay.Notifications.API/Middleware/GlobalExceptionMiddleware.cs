using Microsoft.Extensions.Localization;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;

namespace OdixPay.Notifications.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

    // Middleware constructor — dependencies injected by ASP.NET Core
    public GlobalExceptionMiddleware( RequestDelegate next,
                                      ILogger<GlobalExceptionMiddleware> logger,
                                      IStringLocalizer<SharedResource> IStringLocalizer)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _IStringLocalizer = IStringLocalizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _IStringLocalizer["UnhandledExceptionOccurred"]);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int statusCode = HttpStatusCodes.InternalServerError;
        object? errorPayload = null;
        string message = _IStringLocalizer["AnUnexpectedErrorOccurred"];

        if (ex is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            errorPayload = appEx.ErrorData;
            message = appEx.Message;
        }

        var response = new StandardResponse<object, object>(
            status: ApiConstants.Response.ErrorStatus,
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