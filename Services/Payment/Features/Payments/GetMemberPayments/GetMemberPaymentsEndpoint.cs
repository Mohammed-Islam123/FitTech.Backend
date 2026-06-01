using Carter;
using Payment.Features.Payments.GetMyPayments;
using Payment.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Payment.Features.Payments.GetMemberPayments;

public class GetMemberPaymentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/member/{userId:guid}", Handle)
            .WithName("GetMemberPayments")
            .WithTags("Payments")
            .WithDescription("Admin views payment history for a specific member by their Identity UserId.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<MemberPaymentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        Guid userId,
        GetMemberPaymentsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMemberPaymentsQuery(userId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
