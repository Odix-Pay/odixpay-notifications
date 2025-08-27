using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OdixPay.Notifications.Domain.DTO.Requests
{
    public class CheckUserPermissionForResourceDTO(string userId, string permission)
    {
        [Required(ErrorMessage = "UserId is required")]
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = userId;

        [Required(ErrorMessage = "Permission is required")]
        [StringLength(100)]
        [JsonPropertyName("requiredPermission")]
        public string Permission { get; set; } = permission;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId)
                && !string.IsNullOrWhiteSpace(Permission);
        }
    }
}