using Carter;
using Membership.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Me.Subscriptions;

public class MeSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/subscriptions", Handle)
            .WithName("MeSubscriptions")
            .WithTags("Me")
            .WithDescription("Returns the authenticated member's full subscription history.")
            .RequireAuthorization("MemberOnly")
            .Produces<List<MeSubscriptionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        MeSubscriptionsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
