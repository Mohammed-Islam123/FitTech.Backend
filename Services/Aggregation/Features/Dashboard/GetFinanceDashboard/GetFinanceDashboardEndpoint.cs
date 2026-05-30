using Aggregation.Shared;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Aggregation.Features.Dashboard.GetFinanceDashboard;

public class GetFinanceDashboardEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/finance", Handle)
            .WithName("GetFinanceDashboard")
            .WithTags("Dashboard")
            .WithDescription("Returns finance dashboard: monthly revenue, payments, commission, net profits, lifetime.")
            .RequireAuthorization("AdminOnly")
            .Produces<GetFinanceDashboardResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] GetFinanceDashboardHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(r => Results.Ok(r), errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
