using Aggregation.Shared;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aggregation.Features.Dashboard.GetAdminDashboard;

public class GetAdminDashboardEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/admin", Handle)
            .WithName("GetAdminDashboard")
            .WithTags("Dashboard")
            .WithDescription("Returns aggregated admin dashboard data: members, check-ins, revenue, growth, plans.")
            .RequireAuthorization("AdminOnly")
            .Produces<GetAdminDashboardResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] GetAdminDashboardHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(r => Results.Ok(r), errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
