using ErrorOr;
using Membership.Domain;
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
            .FirstOrDefaultAsync(c => c.CardUid == query.CardUid && c.IsActive, ct);

        if (card is null)
            return Error.NotFound("Card.NotFound", $"No active NFC card with UID '{query.CardUid}' was found.");

        return new GetMemberByCardResponse(
            card.MemberId,
            card.Member.FirstName,
            card.Member.LastName,
            card.Member.Status.ToString()
        );
    }
}

public record GetMemberByCardQuery(string CardUid);

public record GetMemberByCardResponse(
    Guid MemberId,
    string FirstName,
    string LastName,
    string Status
);
