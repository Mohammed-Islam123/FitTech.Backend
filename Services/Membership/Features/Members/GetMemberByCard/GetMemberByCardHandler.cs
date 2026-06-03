using ErrorOr;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.GetMemberByCard;

public class GetMemberByCardHandler(MembershipDbContext context)
{
    public async Task<ErrorOr<GetMemberByCardResponse>> Handle(
        GetMemberByCardQuery query,
        CancellationToken ct)
    {
        var card = await context.NfcCards
            .AsNoTracking()
            .Include(c => c.Member)
                .ThenInclude(m => m.Subscriptions)
                    .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(c => c.CardUid == query.CardUid && c.IsActive, ct);

        if (card is null)
            return Error.NotFound("Card.NotFound", $"No active NFC card with UID '{query.CardUid}' was found.");

        var activeSubscription = card.Member.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartOnUTC)
            .Select(s => new ActiveSubscriptionInfo(
                s.Id,
                s.Plan.Name,
                s.EndOnUTC,
                s.RemainingSessions,
                s.Status.ToString()))
            .FirstOrDefault();

        return new GetMemberByCardResponse(
            card.MemberId,
            card.Member.FirstName,
            card.Member.LastName,
            card.Member.Status.ToString(),
            activeSubscription
        );
    }
}

public record GetMemberByCardQuery(string CardUid);

public record GetMemberByCardResponse(
    Guid MemberId,
    string FirstName,
    string LastName,
    string Status,
    ActiveSubscriptionInfo? ActiveSubscription
);

public record ActiveSubscriptionInfo(
    Guid SubscriptionId,
    string PlanName,
    DateTime? EndOnUTC,
    int? RemainingSessions,
    string Status
);
