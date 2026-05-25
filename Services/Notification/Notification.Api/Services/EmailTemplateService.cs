namespace Notification.Api.Services;

/// <summary>
/// Loads all HTML email templates from disk into memory at startup,
/// then resolves {{Placeholder}} tokens on each call to Render().
/// </summary>
public sealed class EmailTemplateService : IEmailTemplateService
{
    // key = template name (file stem, e.g. "email-confirmation")
    // value = raw HTML content
    private readonly Dictionary<string, string> _templates;

    public EmailTemplateService(IHostEnvironment env, ILogger<EmailTemplateService> logger)
    {
        var templatesPath = Path.Combine(env.ContentRootPath, "Templates");

        if (!Directory.Exists(templatesPath))
            throw new DirectoryNotFoundException(
                $"Email templates directory not found: {templatesPath}");

        _templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.EnumerateFiles(templatesPath, "*.html"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            _templates[name] = File.ReadAllText(file);
            logger.LogInformation("Loaded email template: {TemplateName}", name);
        }

        if (_templates.Count == 0)
            logger.LogWarning("No email templates found in {Path}", templatesPath);
    }

    /// <summary>
    /// Returns the rendered HTML for <paramref name="templateName"/>,
    /// replacing every <c>{{Key}}</c> token with its value from
    /// <paramref name="placeholders"/>.
    /// </summary>
    public string Render(string templateName, Dictionary<string, string> placeholders)
    {
        if (!_templates.TryGetValue(templateName, out var html))
            throw new InvalidOperationException(
                $"Email template '{templateName}' was not found. " +
                $"Available: {string.Join(", ", _templates.Keys)}");

        foreach (var (key, value) in placeholders)
            html = html.Replace($"{{{{{key}}}}}", value,
                StringComparison.OrdinalIgnoreCase);

        return html;
    }
}