using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Infrastructure;
using Payment.Infrastructure.Persistence;
using Shared.Enums;

namespace Payment.Features.Payments.ListPayments;

public class ListPaymentsHandler(
    PaymentDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListPaymentsResponse>>> Handle(
        ListPaymentsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Payment.Unauthorized",
                "Only Administrators can view all payments.");
        }

        var payments = await context.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var results = new List<ListPaymentsResponse>();
        foreach (var p in payments)
        {
            var profile = await identityClient.GetProfileAsync(p.UserId);
            var fullName = profile.IsSuccessStatusCode
                ? $"{profile.Content?.Data?.FirstName} {profile.Content?.Data?.LastName}".Trim()
                : "Unknown";
            var email = profile.IsSuccessStatusCode
                ? profile.Content?.Data?.Email ?? "Unknown"
                : "Unknown";

            results.Add(new ListPaymentsResponse(
                p.Id,
                fullName,
                email,
                p.Amount,
                p.PaymentType.ToString(),
                p.CreatedAt,
                p.PaymentMethod == PaymentMethod.Cash ? "Hand-to-hand" : "Online"
            ));
        }

        return results;
    }
}
