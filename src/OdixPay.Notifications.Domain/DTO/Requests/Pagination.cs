using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class PaginationQueryParams
{

    [JsonPropertyName("page")]
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than zero")]
    public int? Page { get; set; } = 1;

    [JsonPropertyName("limit")]
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than zero")]
    public int? Limit { get; set; } = 20;
}