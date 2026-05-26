using Refit;
using Shared.Wrappers;

namespace Membership.Infrastructure;

/// <description>
/// Refit client for the Courses service. Will be wired in Phase 2 when the Courses service exists.
/// </description>
public interface ICoursesServiceClient
{
    [Get("/api/courses/available")]
    Task<ApiResponse<Response<List<CourseDto>>>> GetAvailableCoursesAsync(Guid memberId);

    [Get("/api/courses/enrolled")]
    Task<ApiResponse<Response<List<CourseDto>>>> GetEnrolledCoursesAsync(Guid memberId);

    [Get("/api/courses/{courseId}")]
    Task<ApiResponse<Response<CourseDetailDto>>> GetCourseDetailAsync(Guid courseId);
}

public class CourseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? CoachName { get; set; }
    public string? Description { get; set; }
}

public class CourseDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int SpotsLeft { get; set; }
    public int Capacity { get; set; }
    public List<SessionSlotDto> Sessions { get; set; } = [];
}

public class SessionSlotDto
{
    public Guid Id { get; set; }
    public string Day { get; set; } = null!;
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public string? Description { get; set; }
}
