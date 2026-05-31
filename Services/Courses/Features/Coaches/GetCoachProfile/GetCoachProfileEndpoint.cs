using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetCoachProfile;

public class GetCoachProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/{coachId:guid}", Handle)
            .WithName("GetCoachProfile")
            .WithTags("Coaches")
            .WithDescription("Returns the public profile of a coach without email or phone number.")
            .RequireAuthorization()
            .Produces<GetCoachProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        Guid coachId,
        GetCoachProfileHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(coachId, ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
