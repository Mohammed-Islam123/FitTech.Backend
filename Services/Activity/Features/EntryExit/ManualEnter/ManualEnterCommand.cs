namespace Activity.Features.EntryExit.ManualEnter;

public record ManualEnterCommand(ManualEnterRequest Request);

public record ManualEnterRequest(Guid MemberId, Guid? CourseId);
