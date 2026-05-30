using MassTransit;
using Notification.Api.Services;
using Shared.Events;

namespace Notification.Api.Consumers;

/// <description>
/// Sends a welcome email with login credentials when a user is registered.
/// For admin-created users, the generated password is included so they can log in.
/// The separate email confirmation link is handled by EmailConfirmationRequestedConsumer.
/// </description>
public class UserRegisteredConsumer(
    IEmailService emailService,
    IEmailTemplateService templateService,
    IConfiguration config,
    ILogger<UserRegisteredConsumer> logger) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;

        var frontendUrl = config["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var loginUrl = $"{frontendUrl}/login";

        // Build the credentials block only if a password was generated (admin-created user)
        var credentialsBlock = @event.GeneratedPassword is not null
            ? $$"""
                <div class="credentials-box">
                  <div class="label">Your login email</div>
                  <div class="value">{{@event.Email}}</div>
                  <div class="label">Temporary password</div>
                  <div class="value">{{@event.GeneratedPassword}}</div>
                  <div class="note">Please change your password after logging in.</div>
                </div>
                """
            : "";

        var placeholders = new Dictionary<string, string>
        {
            ["FirstName"] = @event.FirstName ?? "there",
            ["LoginUrl"] = loginUrl,
            ["CredentialsBlock"] = credentialsBlock
        };

        var html = templateService.Render("welcome", placeholders);

        await emailService.SendEmailAsync(
            @event.Email!,
            @event.FirstName ?? "Valued Member",
            "Welcome to FitTech!",
            html);

        if (@event.GeneratedPassword is not null)
        {
            logger.LogInformation(
                "Welcome email with credentials sent to {Email}",
                @event.Email);
        }
        else
        {
            logger.LogInformation(
                "Welcome email sent to {Email} (self-registered, no credentials included)",
                @event.Email);
        }
    }
}
