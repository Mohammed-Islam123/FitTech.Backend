using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Subscriptions.ListRenewalRequests;

public class ListRenewalRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/subscriptions/renew/pending", Handle)
            .WithName("ListRenewalRequests")
            .WithTags("Subscriptions")
            .WithDescription("Returns all pending cash membership renewal requests for admin review.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<ListRenewalRequestsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        ListRenewalRequestsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new ListRenewalRequestsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
