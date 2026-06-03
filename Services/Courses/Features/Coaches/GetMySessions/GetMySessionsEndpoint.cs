using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetMySessions;

public class GetMySessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/me/sessions", Handle)
            .WithName("GetMySessions")
            .WithTags("Coaches")
            .WithDescription("Returns all sessions across all programs belonging to the currently authenticated coach. Optional date range filters: ?from=YYYY-MM-DD&to=YYYY-MM-DD. Coach is resolved from the JWT token.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetMySessionsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        DateOnly? from,
        DateOnly? to,
        GetMySessionsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMySessionsQuery(from, to), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
