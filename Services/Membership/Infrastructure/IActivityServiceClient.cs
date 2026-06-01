using Refit;

namespace Membership.Infrastructure;

public record ActivitySessionResponse(
    Guid SessionId,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    Guid? CourseId,
    bool IsManual
);

public interface IActivityServiceClient
{
    [Get("/api/activity/members/{memberId}")]
    Task<ApiResponse<List<ActivitySessionResponse>>> GetMemberActivityAsync(Guid memberId);
}
