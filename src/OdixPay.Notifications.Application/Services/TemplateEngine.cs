using OdixPay.Notifications.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace OdixPay.Notifications.Application.Services;

public class TemplateEngine : ITemplateEngine
{
    public string ProcessTemplate(string template, Dictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(template) || variables == null || variables.Count == 0)
            return template;

        var result = template;

        foreach (var variable in variables)
        {
            var pattern = $@"\{{\{{\s*{Regex.Escape(variable.Key)}\s*\}}\}}";
            result = Regex.Replace(result, pattern, variable.Value?.ToString() ?? string.Empty, RegexOptions.IgnoreCase);
        }

        return result;
    }

    public static TemplateEngine Instance { get; } = new TemplateEngine();
}
