using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;

namespace OdixPay.Notifications.API.Models.Response;

public class StandardResponse<T, Y>(string status, T? data, Y? error, string? message)
{


    [JsonPropertyName("status")]
    public string Status { get; set; } = status; // success or error

    [JsonPropertyName("data")]
    public T? Data { get; set; } = data; // success data (if any)

    [JsonPropertyName("error")]
    public Y? ErrorData { get; set; } = error; // error data (if any)

    [JsonPropertyName("message")]
    public string? Message { get; set; } = message; // Response message

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    public static StandardResponse<T, object> Success(T? data, string? message = null)
        => new(ApiConstants.Response.SuccessStatus, data, null, message ??  "Operation completed successfully");

    public static StandardResponse<object, T> Error(T error, string message)
        => new(ApiConstants.Response.ErrorStatus, null, error, message);

    public static StandardResponse<object, T> ValidationError(T error)
        => new(ApiConstants.Response.ErrorStatus, null, error, "Validation Failed");
}