namespace Shared.Events;

/// <description>
/// Generic email notification event. The publisher (Membership, Courses, etc.) renders
/// the full email content with all business context it owns. The Notification service
/// acts as a dumb pipe — just delivers the email without any business logic.
/// </description>
public record SendEmailEvent(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true
);
