using Refit;
using Shared.Wrappers;

namespace Activity.Infrastructure;

public interface IMembershipServiceClient
{
    [Get("/api/members/{memberId}")]
    Task<ApiResponse<MemberValidationResponse>> GetMemberAsync(Guid memberId);
}

public class MemberValidationResponse
{
    public Guid MemberId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Status { get; set; } = null!;
}
