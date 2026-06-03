using MassTransit;
using Notification.Api.Services;
using Shared.Events;

namespace Notification.Api.Consumers;

/// <description>
/// Consumes SendEmailEvent from any service (Membership, Courses, Equipments, etc.)
/// and delivers the email using the configured IEmailService (SMTP).
/// The publisher renders the full email content — this consumer is a dumb pipe.
/// </description>
public class SendEmailEventConsumer(
    IEmailService emailService,
    ILogger<SendEmailEventConsumer> logger) : IConsumer<SendEmailEvent>
{
    public async Task Consume(ConsumeContext<SendEmailEvent> context)
    {
        var @event = context.Message;

        if (string.IsNullOrWhiteSpace(@event.To))
        {
            logger.LogWarning("SendEmailEvent skipped: no recipient");
            return;
        }

        await emailService.SendEmailAsync(
            @event.To,
            @event.To,
            @event.Subject,
            @event.Body);

        logger.LogInformation(
            "Email sent to {To} with subject {Subject}",
            @event.To,
            @event.Subject);
    }
}
