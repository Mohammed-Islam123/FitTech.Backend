using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Features.Payments.GetMyPayments;
using Payment.Infrastructure.Persistence;

namespace Payment.Features.Payments.GetMemberPayments;

/// <description>
/// Admin views payment history for a specific member by their Identity UserId.
/// </description>
public class GetMemberPaymentsHandler(
    PaymentDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<MemberPaymentResponse>>> Handle(
        GetMemberPaymentsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Payment.Unauthorized", "Only Administrators can view member payments.");

        return await context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == query.UserId)
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
