using ErrorOr;
using Wolverine.Shims.MediatR;

namespace Membership.Features.Members.GetMember;

public record GetMemberQuery(Guid MemberId) : IRequest<ErrorOr<GetMemberResponse>>;
