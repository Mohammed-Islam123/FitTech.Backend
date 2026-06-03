using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.GetProgramSessions;

public class GetProgramSessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/{programId:guid}/sessions", Handle)
            .WithName("GetProgramSessions")
            .WithTags("Programs")
            .WithDescription("Returns all generated sessions for a program with enrolled members and their attendance status. Future sessions exclude attendance data.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<GetProgramSessionsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        GetProgramSessionsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetProgramSessionsQuery(programId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
