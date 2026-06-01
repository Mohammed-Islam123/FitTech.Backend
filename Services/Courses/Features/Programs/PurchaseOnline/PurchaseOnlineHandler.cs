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

namespace Courses.Features.Programs.PurchaseOnline;

/// <description>
/// Member purchases a program online (credit card). Validates program status, capacity,
/// and amount. Processes payment immediately (simulated confirm), creates enrollment,
/// and sends a thank-you email. Free programs skip the payment step.
/// </description>
public class PurchaseOnlineHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<PurchaseOnlineResponse>> Handle(
        PurchaseOnlineCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Purchase.Unauthorized", "Only Members can purchase programs.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Purchase.Unauthorized", "Authentication required.");

        var program = await context.Programs
            .AsNoTracking()
            .Include(p => p.Coach)
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, ct);

        if (program is null)
            return Error.NotFound("Program.NotFound", "The specified program does not exist.");

        if (program.Status != ProgramStatus.Accepted)
            return Error.Validation("Program.NotAccepted", "You can only purchase programs that have been accepted.");

        if (program.MaxParticipants > 0 && program.Enrollments.Count >= program.MaxParticipants)
            return Error.Validation("Program.Full", "This program has no available spots left.");

        var alreadyEnrolled = program.Enrollments.Any(e => e.MemberId == userId.Value);
        if (alreadyEnrolled)
            return Error.Conflict("Program.AlreadyEnrolled", "You are already enrolled in this program.");

        var req = command.Request;
        if (req.Amount != program.TotalPrice)
            return Error.Validation("Payment.AmountMismatch",
                $"The purchase amount must match the program price of {program.TotalPrice} DZD.");

        // Process payment (skip if free)
        Guid? paymentId = null;
        var paymentMethod = program.TotalPrice == 0 ? "Free" : PaymentMethod.CreditCard.ToString();

        if (program.TotalPrice > 0)
        {
            var paymentPayload = new CreatePaymentRequest(
                UserId: userId.Value,
                Amount: program.TotalPrice,
                PaymentMethod: PaymentMethod.CreditCard.ToString(),
                PaymentType: "CoursePurchase",
                ReferenceId: program.Id,
                Notes: req.Notes);

            var paymentResponse = await paymentClient.CreatePaymentAsync(paymentPayload);
            if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
                return Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.");

            paymentId = paymentResponse.Content.PaymentId;

            var confirmResponse = await paymentClient.ConfirmPaymentAsync(paymentId.Value);
            if (!confirmResponse.IsSuccessStatusCode)
                return Error.Failure("Payment.ConfirmFailed", "Failed to confirm payment.");
        }

        // Record audit request
        var purchaseRequest = new CoursePurchaseRequest
        {
            Id = Guid.CreateVersion7(),
            MemberId = userId.Value,
            ProgramId = program.Id,
            Amount = program.TotalPrice,
            PaymentMethod = paymentMethod,
            Status = CoursePurchaseRequestStatus.Accepted,
            Notes = req.Notes,
            ResolvedAt = DateTime.UtcNow
        };
        context.CoursePurchaseRequests.Add(purchaseRequest);

        // Create enrollment
        var enrollment = new ProgramEnrollment
        {
            Id = Guid.CreateVersion7(),
            ProgramId = program.Id,
            MemberId = userId.Value,
            EnrolledAt = DateTime.UtcNow
        };
        context.ProgramEnrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        // Send thank-you email
        await messageBus.PublishAsync(new SendEmailEvent(
            To: $"member-{userId.Value}",
            Subject: $"Enrollment Confirmed - {program.Name}",
            Body: $"<h2>Thank You for Your Purchase!</h2>" +
                  $"<p>Your enrollment in <strong>{program.Name}</strong> has been confirmed.</p>" +
                  $"<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;'>" +
                  $"<tr><td><strong>Program</strong></td><td>{program.Name}</td></tr>" +
                  $"<tr><td><strong>Coach</strong></td><td>{program.Coach.FirstName} {program.Coach.LastName}</td></tr>" +
                  $"<tr><td><strong>Amount Paid</strong></td><td>{program.TotalPrice} DZD</td></tr>" +
                  $"<tr><td><strong>Payment Method</strong></td><td>{paymentMethod}</td></tr>" +
                  $"<tr><td><strong>Purchase Date</strong></td><td>{DateTime.UtcNow:yyyy-MM-dd}</td></tr>" +
                  $"</table>" +
                  $"<p>Thank you for choosing FitTech!</p>"
        ));

        return new PurchaseOnlineResponse(
            EnrollmentId: enrollment.Id,
            ProgramId: program.Id,
            ProgramName: program.Name,
            PaymentId: paymentId,
            Amount: program.TotalPrice,
            PaymentMethod: paymentMethod,
            PurchasedAt: DateTime.UtcNow
        );
    }
}
