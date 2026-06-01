using Carter;
using Membership.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Me.Payments;

public class MePaymentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/payments", Handle)
            .WithName("MePayments")
            .WithTags("Me")
            .WithDescription("Returns the authenticated member's payment history.")
            .RequireAuthorization("MemberOnly")
            .Produces<List<MePaymentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        MePaymentsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
