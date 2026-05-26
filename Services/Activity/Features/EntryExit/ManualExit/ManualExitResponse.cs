namespace Activity.Features.EntryExit.ManualExit;

public record ManualExitResponse(Guid SessionId, DateTime CheckOutTime);
