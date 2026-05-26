namespace Notification.Api.Services;

public interface IEmailTemplateService
{
    string Render(string templateName, Dictionary<string, string> placeholders);
}