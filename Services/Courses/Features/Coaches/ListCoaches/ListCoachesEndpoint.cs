using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.ListCoaches;

public class ListCoachesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches", Handle)
            .WithName("ListCoaches")
            .WithTags("Coaches")
            .WithDescription("Returns a paginated list of all coaches with admin-level detail including contact info.")
            .RequireAuthorization("AdminOnly")
            .Produces<ListCoachesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        int? page,
        int? pageSize,
        ListCoachesHandler handler,
        CancellationToken ct)
    {
        var query = new ListCoachesQuery(
            Page: page ?? 1,
            PageSize: pageSize ?? 10
        );

        var result = await handler.Handle(query, ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
