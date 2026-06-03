using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetMyPrograms;

public class GetMyProgramsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/me/programs", Handle)
            .WithName("GetMyPrograms")
            .WithTags("Coaches")
            .WithDescription("Returns all programs for the currently authenticated coach, including all statuses: Pending, Accepted, and Rejected. Coach is resolved from the JWT token.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetMyProgramsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        GetMyProgramsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMyProgramsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
