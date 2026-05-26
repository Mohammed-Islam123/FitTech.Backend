namespace Shared.Events;

public record ProgramCreatedEvent(
    Guid ProgramId,
    string ProgramName,
    Guid CoachId,
    string CoachName,
    DateTime CreatedAt
);
