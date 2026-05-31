using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Sessions.GetSessionHistory;

public class GetSessionHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sessions/history", Handle)
            .WithName("GetSessionHistory")
            .WithTags("Sessions")
            .WithDescription("Returns session history for the authenticated member within a date range, including attendance status per session.")
            .RequireAuthorization("MemberOnly")
            .Produces<GetSessionHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        GetSessionHistoryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetSessionHistoryQuery(startDate, endDate), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
