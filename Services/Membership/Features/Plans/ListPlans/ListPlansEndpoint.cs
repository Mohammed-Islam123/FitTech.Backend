using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Plans.ListPlans;

public class ListPlansEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plans", Handle)
            .WithName("ListPlans")
            .WithTags("Plans")
            .WithDescription("Retrieves all subscription plans. Restricted to Administrators.")
            .Produces<List<ListPlansResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        ListPlansHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new ListPlansQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
