namespace OdixPay.Notifications.Domain.Utils;

public static class SlugifyString
{
    public static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Convert to lower case
        input = input.ToLowerInvariant();

        // Replace spaces and special characters with hyphens
        var slug = System.Text.RegularExpressions.Regex.Replace(input, @"[^\w\s-]", string.Empty);
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s-]+", "-"); // Replace multiple spaces or hyphens with a single hyphen
        // Trim hyphens from the start and end
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');

        return slug;
    }
}