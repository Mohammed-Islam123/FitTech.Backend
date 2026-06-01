using Carter;
using Payment.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Payment.Features.Payments.GetMyPayments;

public class GetMyPaymentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/member/me", Handle)
            .WithName("GetMyPayments")
            .WithTags("Payments")
            .WithDescription("Returns the authenticated member's payment history.")
            .RequireAuthorization()
            .Produces<List<MemberPaymentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        GetMyPaymentsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMyPaymentsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
