using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.GetAvailablePrograms;

public class GetAvailableProgramsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/available", Handle)
            .WithName("GetAvailablePrograms")
            .WithTags("Programs")
            .WithDescription("Returns accepted programs the authenticated member is not enrolled in.")
            .RequireAuthorization()
            .Produces<List<GetAvailableProgramsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        GetAvailableProgramsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
