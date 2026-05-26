namespace Shared.Events;

public record ProgramRejectedEvent(
    Guid ProgramId,
    string ProgramName,
    Guid CoachId,
    string CoachName,
    DateTime RejectedAt
);
