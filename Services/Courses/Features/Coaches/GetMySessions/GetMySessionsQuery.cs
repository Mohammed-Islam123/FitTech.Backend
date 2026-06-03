namespace Courses.Features.Coaches.GetMySessions;

public record GetMySessionsQuery(DateOnly? From = null, DateOnly? To = null);
