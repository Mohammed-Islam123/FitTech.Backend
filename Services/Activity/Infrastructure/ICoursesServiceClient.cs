using Refit;
using Shared.Wrappers;

namespace Activity.Infrastructure;

public interface ICoursesServiceClient
{
    [Get("/api/programs/{programId}/members")]
    Task<ApiResponse<Response<List<CourseMemberDto>>>> GetProgramMembersAsync(Guid programId);
}

public class CourseMemberDto
{
    public Guid MemberId { get; set; }
    public string FullName { get; set; } = null!;
}
