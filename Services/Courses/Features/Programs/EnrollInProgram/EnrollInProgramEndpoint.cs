using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.EnrollInProgram;

public class EnrollInProgramEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/programs/{programId:guid}/enroll", Handle)
            .WithName("EnrollInProgram")
            .WithTags("Programs")
            .WithDescription("Enrolls the authenticated member in a specific program.")
            .RequireAuthorization("MemberOnly")
            .Produces<EnrollInProgramResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        EnrollInProgramHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new EnrollInProgramCommand(programId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
