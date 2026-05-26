namespace Courses.Features.Programs.ListProgramRequests;

public record ListProgramRequestsResponse(
    Guid ProgramId,
    string ProgramName,
    string? Description,
    string CoachName
);
