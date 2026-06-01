using Carter;
using ErrorOr;
using Courses.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.ListPurchaseRequests;

public class ListPurchaseRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/purchase/pending", Handle)
            .WithName("ListPurchaseRequests")
            .WithTags("Programs")
            .WithDescription("Returns all pending cash course purchase requests for admin review.")
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
