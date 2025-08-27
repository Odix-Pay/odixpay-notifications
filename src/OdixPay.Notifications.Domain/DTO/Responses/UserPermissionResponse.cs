using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Responses;

public class UserPermissionResponseDTO
{
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPermissionData Data { get; set; } = new UserPermissionData();

}

public class UserPermissionData
{
    [JsonPropertyName("hasPermission")]
    public bool HasPermission { get; set; }
}