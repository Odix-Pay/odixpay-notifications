using System.Net;
using System.Text.Json;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Application.Exceptions;

namespace OdixPay.Notifications.API.Middleware;

public class StandardResponseMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip middleware for Swagger-related requests
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && (path.StartsWith(ApiConstants.Paths.Swagger) || path.StartsWith(ApiConstants.Paths.VersionedSwagger)))
        {
            await _next(context);
            return;
        }

        // Store the original response body stream
        var originalBodyStream = context.Response.Body;

        // Replace the response body with a memory stream to capture output
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // Call the next middleware/controller
            await _next(context);

            // Skip middleware for non-JSON responses (e.g., static files)
            if (context.Response.ContentType != null &&
                !context.Response.ContentType.Contains("application/json"))
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                return;
            }

            // Special handling for 404 (not matched by any endpoint)
            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound && string.IsNullOrWhiteSpace(context.Response.ContentType))
            {
                context.Response.Body = originalBodyStream;
                context.Response.ContentType = "application/json";

                var res = StandardResponse<object, object>.Error(
                    ApiConstants.ErrorTypes.NotFound,
                    "The requested resource was not found."
                );

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync(JsonSerializer.Serialize(res));
                return;
            }

            // Read the response body
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);


            // If response content is already in instance of StandardResponse, no need to process again. Just return response.
            try
            {
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                if (root.TryGetProperty("status", out var Status) &&
                    root.TryGetProperty("timestamp", out _))
                {
                    await responseBody.CopyToAsync(originalBodyStream);
                    return;
                }
            }
            catch
            {
                // Not a StandardResponse, continue with normal processing
            }

            // Prepare the standard response
            object? data = null;
            object? error = null;
            string? message = "Operation completed successfully";
            string status = ApiConstants.Response.SuccessStatus;

            if (context.Response.StatusCode >= 400)
            {
                status = ApiConstants.Response.ErrorStatus;
                error = GetErrorType(context.Response.StatusCode);
                var errorData = GetErrorData(context.Response.StatusCode, responseContent);

                message = errorData?.Message;
                error = errorData?.ErrorData;
            }
            else if (!string.IsNullOrEmpty(responseContent))
            {
                try
                {
                    data = JsonSerializer.Deserialize<object>(responseContent);
                    message = "Operation completed successfully";
                }
                catch
                {
                    data = responseContent; // Fallback to raw content if not JSON
                    message = "Operation completed successfully";
                }
            }

            var standardResponse = new StandardResponse<object, object>(status, data, error, message);

            // Write the standard response
            context.Response.Body = originalBodyStream;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(standardResponse));
        }
        catch (Exception ex)
        {
            object? error = "InternalServerError";
            string message = $"An unexpected error occurred: {ex.Message}";
            int statusCode = (int)HttpStatusCode.InternalServerError;

            if (ex is AppException appEx)
            {
                error = appEx.ErrorData;
                message = appEx.Message;
                statusCode = appEx.StatusCode;
            }

            // Handle unhandled exceptions
            context.Response.Body = originalBodyStream;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var standardResponse = StandardResponse<object, dynamic>.Error(
                error!,
                message
            );

            await context.Response.WriteAsync(JsonSerializer.Serialize(standardResponse));
        }
    }

    // Converts a given http error into a human readable text.
    private static string GetErrorType(int statusCode) => statusCode switch
    {
        (int)HttpStatusCode.Unauthorized => ApiConstants.ErrorTypes.Unauthorized,
        (int)HttpStatusCode.Forbidden => ApiConstants.ErrorTypes.Forbidden,
        (int)HttpStatusCode.NotFound => ApiConstants.ErrorTypes.NotFound,
        (int)HttpStatusCode.BadRequest => ApiConstants.ErrorTypes.BadRequest,
        (int)HttpStatusCode.UnprocessableEntity => ApiConstants.ErrorTypes.ValidationError,
        (int)HttpStatusCode.Conflict => ApiConstants.ErrorTypes.Conflict,
        _ => ApiConstants.ErrorTypes.InternalServerError
    };

    // Gets standard response message to return to server
    private static AppException GetErrorData(int statusCode, string responseContent)
    {
        if (!string.IsNullOrEmpty(responseContent))
        {
            try
            {
                var errorObj = JsonSerializer.Deserialize<object>(responseContent);

                var message = "An error occurred";

                return new AppException(
                    message,
                    statusCode,
                    errorData: errorObj ?? null
                );
            }
            catch
            {
                return new AppException(
                    message: "An error occured",
                    statusCode,
                    errorData: responseContent
                ); // Fallback to raw content
            }
        }

        return new AppException(
            message: "An error occured",
            statusCode,
            errorData: null
        );
    }
}

// Extension method to register the middleware
public static class StandardResponseMiddlewareExtensions
{
    public static IApplicationBuilder UseStandardResponse(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<StandardResponseMiddleware>();
    }
}