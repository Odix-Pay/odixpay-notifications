using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Requests;

public class TemplateVariableStructure
{
    [Required(ErrorMessage = "Type is required.")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required is required.")]
    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    [JsonPropertyName("description")]
    public string? Description { get; set; } = string.Empty;
}