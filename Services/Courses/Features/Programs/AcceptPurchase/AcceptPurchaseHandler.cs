using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Courses.Infrastructure;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Events;
using Wolverine;

namespace Courses.Features.Programs.AcceptPurchase;

/// <description>
/// Admin accepts a cash course purchase request. Records payment (if amount > 0),
/// creates the program enrollment, and sends a thank-you email.
/// </description>
public class AcceptPurchaseHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<AcceptPurchaseResponse>> Handle(
        AcceptPurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Purchase.Unauthorized", "Only Administrators can accept purchase requests.");

        var request = await context.CoursePurchaseRequests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Purchase request not found.");

        if (request.Status != CoursePurchaseRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "This request has already been resolved.");

        var program = await context.Programs
            .Include(p => p.Coach)
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramId, ct);

        if (program is null)
            return Error.NotFound("Program.NotFound", "The program for this purchase no longer exists.");

        // Record payment if amount > 0
        Guid? paymentId = null;
        if (request.Amount > 0)
        {
            var paymentPayload = new CreatePaymentRequest(
                UserId: request.MemberId,
                Amount: request.Amount,
                PaymentMethod: PaymentMethod.Cash.ToString(),
                PaymentType: PaymentType.CoursePurchase.ToString(),
                ReferenceId: program.Id,
                Notes: command.Notes ?? request.Notes);

            var paymentResponse = await paymentClient.CreatePaymentAsync(paymentPayload);
            if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
                return Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.");

            paymentId = paymentResponse.Content.PaymentId;
        }

        // Create enrollment
        var enrollment = new ProgramEnrollment
        {
            Id = Guid.CreateVersion7(),
            ProgramId = program.Id,
            MemberId = request.MemberId,
            EnrolledAt = DateTime.UtcNow
        };

        context.ProgramEnrollments.Add(enrollment);
        request.Status = CoursePurchaseRequestStatus.Accepted;
        request.ResolvedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        // Send thank-you email
        await messageBus.PublishAsync(new SendEmailEvent(
            To: $"member-{request.MemberId}",
            Subject: $"Enrollment Confirmed - {program.Name}",
            Body: $"<h2>Thank You for Your Purchase!</h2>" +
                  $"<p>Your enrollment in <strong>{program.Name}</strong> has been confirmed.</p>" +
                  $"<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;'>" +
                  $"<tr><td><strong>Program</strong></td><td>{program.Name}</td></tr>" +
                  $"<tr><td><strong>Coach</strong></td><td>{program.Coach.FirstName} {program.Coach.LastName}</td></tr>" +
                  $"<tr><td><strong>Amount Paid</strong></td><td>{request.Amount} DZD</td></tr>" +
                  $"<tr><td><strong>Payment Method</strong></td><td>Cash (Hand-to-Hand)</td></tr>" +
                  $"<tr><td><strong>Purchase Date</strong></td><td>{DateTime.UtcNow:yyyy-MM-dd}</td></tr>" +
                  $"</table>" +
                  $"<p>Thank you for choosing FitTech!</p>"
        ));

        return new AcceptPurchaseResponse(request.Id, enrollment.Id, request.Status);
    }
}
