using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.GetProgramRequest;

public class GetProgramRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/requests/{programId:guid}", Handle)
            .WithName("GetProgramRequest")
            .WithTags("Programs")
            .WithDescription("Returns full details of a program creation request for admin review.")
            .RequireAuthorization("AdminOnly")
            .Produces<GetProgramRequestResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        GetProgramRequestHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetProgramRequestQuery(programId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
