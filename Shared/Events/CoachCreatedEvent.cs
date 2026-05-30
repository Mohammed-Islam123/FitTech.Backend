namespace Shared.Events;

public record CoachCreatedEvent(
    Guid CoachId,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email
);
