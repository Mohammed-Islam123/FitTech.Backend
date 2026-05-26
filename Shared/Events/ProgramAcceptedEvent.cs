namespace Shared.Events;

public record ProgramAcceptedEvent(
    Guid ProgramId,
    string ProgramName,
    Guid CoachId,
    string CoachName,
    DateTime AcceptedAt
);
