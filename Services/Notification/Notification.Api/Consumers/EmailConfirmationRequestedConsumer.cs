using MassTransit;
using Notification.Api.Services;
using Shared.Events;

namespace Notification.Api.Consumers;

public class EmailConfirmationRequestedConsumer(
    IEmailService emailService,
    IEmailTemplateService templateService,
    IConfiguration config) : IConsumer<EmailConfirmationRequestedEvent>
{
    public async Task Consume(ConsumeContext<EmailConfirmationRequestedEvent> context)
    {
        var @event = context.Message;

        var confirmUrl = $"{config["Frontend:BaseUrl"]}/confirm-email" +
                         $"?userId={@event.UserId}" +
                         $"&token={Uri.EscapeDataString(@event.Token)}";

        var html = templateService.Render("email-confirmation", new Dictionary<string, string>
        {
            ["FirstName"]  = @event.FirstName,
            ["ConfirmUrl"] = confirmUrl,
        });

        await emailService.SendEmailAsync(
            @event.Email,
            @event.FirstName,
            "Confirm your FitTech email",
            html);
    }
}