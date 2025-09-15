using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Utils;

namespace OdixPay.Notifications.Domain.Entities;

public class NotificationTemplate : BaseEntity
{
    public required string Name { get; set; } = string.Empty; //Unique name for the template
    public string Slug { get; private set; } = string.Empty; // URL-friendly identifier. Generated from Name. Unique.
    public required NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Variables { get; set; }

    public string Locale { get; set; } = "en"; // Language/locale of the template, e.g., "en", "fr", "es" - Defaults to "en"

    public void GenerateSlug()
    {
        Slug = SlugifyString.Slugify(Name);
    }
}