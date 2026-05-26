namespace Membership.Features.Courses.GetCourseDetail;

public record GetCourseDetailResponse(
    Guid Id,
    string Name,
    decimal Price,
    string? Description,
    int SpotsLeft,
    int Capacity,
    List<CourseSessionResponse> Sessions
);

public record CourseSessionResponse(
    Guid Id,
    string Day,
    string StartTime,
    string EndTime,
    string? Description
);
