using Refit;
using Shared.Wrappers;

namespace Activity.Infrastructure;

public interface IMembershipServiceClient
{
    [Get("/api/members/{memberId}")]
    Task<ApiResponse<MemberValidationResponse>> GetMemberAsync(Guid memberId);

    [Get("/api/members/by-card/{cardUid}")]
    Task<ApiResponse<MemberByCardResponse>> GetMemberByCardAsync(string cardUid);

    [Post("/api/members/track-entry")]
    Task<ApiResponse<TrackEntryResponse>> TrackMemberEntryAsync([Body] TrackEntryRequest request);
}

public class MemberValidationResponse
{
    public Guid MemberId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class MemberByCardResponse
{
    public Guid MemberId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public ActiveSubscriptionInfo? ActiveSubscription { get; set; }
}

public class ActiveSubscriptionInfo
{
    public Guid SubscriptionId { get; set; }
    public string PlanName { get; set; } = null!;
    public DateTime? EndOnUTC { get; set; }
    public int? RemainingSessions { get; set; }
    public string Status { get; set; } = null!;
}

public record TrackEntryRequest(Guid MemberId);

public class TrackEntryResponse
{
    public bool Success { get; set; }
    public string MemberName { get; set; } = null!;
    public int? RemainingSessions { get; set; }
    public ActiveSubscriptionInfo? ActiveSubscription { get; set; }
}
