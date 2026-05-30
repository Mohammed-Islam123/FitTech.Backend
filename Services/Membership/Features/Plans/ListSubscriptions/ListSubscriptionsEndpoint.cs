using Carter;
using ErrorOr;
using Membership.Features.Plans.ListPlans;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Plans.ListSubscriptions;

public class ListSubscriptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/subscriptions", Handle)
            .WithName("ListSubscriptions")
            .WithTags("Plans")
            .WithDescription("Returns all subscription plans with name, price, status, session count, and duration.")
            .Produces<List<ListPlansResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        ListPlansHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(
            new ListPlansQuery(), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
