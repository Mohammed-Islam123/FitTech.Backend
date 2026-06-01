using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Infrastructure.Persistence;

namespace Payment.Features.Payments.GetMyPayments;

/// <description>
/// Returns payment history for the authenticated member. Reads UserId from JWT,
/// queries payments without calling Identity for enrichment (member already knows
/// their own details).
/// </description>
public class GetMyPaymentsHandler(
    PaymentDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<MemberPaymentResponse>>> Handle(
        GetMyPaymentsQuery query,
        CancellationToken ct)
    {
        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Payment.Unauthorized", "Authentication required.");

        return await context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId.Value)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new MemberPaymentResponse(
                p.Id,
                p.Amount,
                p.PaymentMethod.ToString(),
                p.PaymentType.ToString(),
                p.ReferenceId,
                p.Status,
                p.CreatedAt))
            .ToListAsync(ct);
    }
}
