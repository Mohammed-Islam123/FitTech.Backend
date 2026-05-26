using ErrorOr;

namespace Membership.Features.Courses.GetCourseDetail;

/// <description>
/// Returns full details of a specific course.
/// Currently returns not-found until the Courses service is built in Phase 2.
/// </description>
public class GetCourseDetailHandler
{
    public Task<ErrorOr<GetCourseDetailResponse>> Handle(
        GetCourseDetailQuery query,
        CancellationToken ct)
    {
        return Task.FromResult<ErrorOr<GetCourseDetailResponse>>(
            Error.NotFound(
                "Course.NotFound",
                "Course details are not yet available. The Courses service will be implemented in Phase 2."));
    }
}
