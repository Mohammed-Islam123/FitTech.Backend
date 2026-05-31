using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.GetProgramDetail;

public class GetProgramDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/{programId:guid}", Handle)
            .WithName("GetProgramDetail")
            .WithTags("Programs")
            .WithDescription("Returns the full details of a specific program including time slots and coach info.")
            .RequireAuthorization()
            .Produces<GetProgramDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        GetProgramDetailHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(programId, ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
