using ErrorOr;
using Wolverine.Shims.MediatR;

namespace Membership.Features.Members.ListMembers;

public record ListMembersQuery(ListMembersRequest Request) : IRequest<ErrorOr<ListMembersResponse>>;
