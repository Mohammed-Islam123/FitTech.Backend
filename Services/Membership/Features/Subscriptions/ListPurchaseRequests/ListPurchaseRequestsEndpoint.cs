using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Subscriptions.ListPurchaseRequests;

public class ListPurchaseRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/subscriptions/purchase/pending", Handle)
            .WithName("ListPurchaseRequests")
            .WithTags("Subscriptions")
            .WithDescription("Returns all pending cash plan purchase requests for admin review.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<ListPurchaseRequestsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        ListPurchaseRequestsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new ListPurchaseRequestsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
