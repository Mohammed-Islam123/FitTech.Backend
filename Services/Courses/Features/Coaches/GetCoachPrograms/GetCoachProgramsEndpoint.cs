using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetCoachPrograms;

public class GetCoachProgramsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/{coachId:guid}/programs", Handle)
            .WithName("GetCoachPrograms")
            .WithTags("Coaches")
            .WithDescription("Returns all programs for a specific coach, including inactive and pending ones.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetCoachProgramsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid coachId,
        GetCoachProgramsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetCoachProgramsQuery(coachId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
