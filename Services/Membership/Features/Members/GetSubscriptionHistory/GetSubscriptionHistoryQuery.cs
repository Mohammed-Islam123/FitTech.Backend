using ErrorOr;
using Wolverine.Shims.MediatR;

namespace Membership.Features.Members.GetSubscriptionHistory;

public record GetSubscriptionHistoryQuery(Guid MemberId) : IRequest<ErrorOr<SubscriptionHistoryResponse>>;
