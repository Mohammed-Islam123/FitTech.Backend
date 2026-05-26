namespace Activity.Features.EntryExit.ManualExit;

public record ManualExitCommand(ManualExitRequest Request);

public record ManualExitRequest(Guid MemberId, Guid? CourseId);
