namespace Courses.Features.Sessions.GetSessionHistory;

public record GetSessionHistoryQuery(DateOnly StartDate, DateOnly EndDate);
