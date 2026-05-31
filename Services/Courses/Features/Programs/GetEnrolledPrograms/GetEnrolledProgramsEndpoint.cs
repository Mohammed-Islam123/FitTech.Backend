using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.GetEnrolledPrograms;

public class GetEnrolledProgramsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/enrolled", Handle)
            .WithName("GetEnrolledPrograms")
            .WithTags("Programs")
            .WithDescription("Returns programs the authenticated member is enrolled in.")
            .RequireAuthorization("MemberOnly")
            .Produces<List<GetEnrolledProgramsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        GetEnrolledProgramsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
