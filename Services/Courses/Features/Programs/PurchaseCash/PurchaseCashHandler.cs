using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.PurchaseCash;

/// <description>
/// Member submits a cash purchase request for a program. Creates a CoursePurchaseRequest
/// with status Pending for admin review. Validates program exists, is accepted, has capacity,
/// member is not already enrolled, and amount matches program price. Free programs must use
/// the online endpoint.
/// </description>
public class PurchaseCashHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<PurchaseCashResponse>> Handle(
        PurchaseCashCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Purchase.Unauthorized", "Only Members can submit purchase requests.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Purchase.Unauthorized", "Authentication required.");

        var program = await context.Programs
            .AsNoTracking()
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, ct);

        if (program is null)
            return Error.NotFound("Program.NotFound", "The specified program does not exist.");

        if (program.Status != ProgramStatus.Accepted)
            return Error.Validation("Program.NotAccepted", "You can only purchase programs that have been accepted.");

        if (program.TotalPrice == 0)
            return Error.Validation("Program.Free",
                "This program is free. Use the online purchase endpoint instead.");

        if (program.MaxParticipants > 0 && program.Enrollments.Count >= program.MaxParticipants)
            return Error.Validation("Program.Full", "This program has no available spots left.");

        var alreadyEnrolled = program.Enrollments.Any(e => e.MemberId == userId.Value);
        if (alreadyEnrolled)
            return Error.Conflict("Program.AlreadyEnrolled", "You are already enrolled in this program.");

        var req = command.Request;
        if (req.Amount != program.TotalPrice)
            return Error.Validation("Payment.AmountMismatch",
                $"The purchase amount must match the program price of {program.TotalPrice} DZD.");

        var purchaseRequest = new CoursePurchaseRequest
        {
            Id = Guid.CreateVersion7(),
            MemberId = userId.Value,
            ProgramId = program.Id,
            Amount = req.Amount,
            PaymentMethod = "Cash",
            Status = CoursePurchaseRequestStatus.Pending,
            Notes = req.Notes
        };

        context.CoursePurchaseRequests.Add(purchaseRequest);
        await context.SaveChangesAsync(ct);

        return new PurchaseCashResponse(purchaseRequest.Id, purchaseRequest.Status);
    }
}
