namespace Membership.Features.Members.SuspendMember;

public record SuspendMemberResponse(Guid MemberId, string Status);
