using MassTransit;
using Notification.Api.Services;
using Shared.Events;

namespace Notification.Api.Consumers;

public class EmailConfirmationRequestedConsumer(
    IEmailService emailService,
    IEmailTemplateService templateService,
    IConfiguration config,
    IHostEnvironment hostEnvironment,
    ILogger<EmailConfirmationRequestedConsumer> logger) : IConsumer<EmailConfirmationRequestedEvent>
{
    public async Task Consume(ConsumeContext<EmailConfirmationRequestedEvent> context)
    {
        var @event = context.Message;

        var confirmUrl = $"{config["Frontend:BaseUrl"]}/confirm-email" +
                         $"?userId={@event.UserId}" +
                         $"&token={Uri.EscapeDataString(@event.Token)}";

        var html = templateService.Render("email-confirmation", new Dictionary<string, string>
        {
            ["FirstName"] = @event.FirstName,
            ["ConfirmUrl"] = confirmUrl,
        });

        await emailService.SendEmailAsync(
            @event.Email,
            @event.FirstName,
            "Confirm your FitTech email",
            html);

        if (hostEnvironment.IsDevelopment())
        {
            var identityUrl = config["Identity:BaseUrl"] ?? "http://localhost:5051";
            var rawToken = @event.Token;
            var body = $$"""{"userId":"{{@event.UserId}}","token":"{{rawToken}}"}""";

            logger.LogInformation(
                """
                ┌───────────────────────────────────────────────────────────────────────────┐
                │ DEV MODE: Verify email via curl (no frontend needed)                    │
                └───────────────────────────────────────────────────────────────────────────┘
                curl -L  -X POST {IdentityUrl}/api/User/verify-email -H "Content-Type: application/json" -d '{Body}'
                """,
                identityUrl, body);
        }
    }
}
