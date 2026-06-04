using Membership.Domain.Enums;

namespace Membership.Features.Members.ListMembers;

public record ListMembersResponse(
    List<MemberSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record MemberSummaryDto(
    Guid MemberId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? ProfilePhotoUrl,
    MemberStatus Status,
    DateTime JoinDate,
    string? ActivePlanName);
