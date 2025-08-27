using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Responses;

/// <summary>
/// Response from getting a card's transaction history.
/// </summary>
public class PaginatedResponseDTO<T>
{
    [Required]
    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; set; }

    [Required]
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [Required]
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [Required]
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}