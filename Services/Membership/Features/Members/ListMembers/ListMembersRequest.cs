using Membership.Domain.Enums;

namespace Membership.Features.Members.ListMembers;

public class ListMembersRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public MemberStatus? Status { get; set; }
}
