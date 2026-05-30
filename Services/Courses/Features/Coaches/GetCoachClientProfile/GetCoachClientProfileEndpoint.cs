using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetCoachClientProfile;

public class GetCoachClientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/{coachId:guid}/clients/{memberId:guid}", Handle)
            .WithName("GetCoachClientProfile")
            .WithTags("Coaches")
            .WithDescription("Returns the full profile of a specific member enrolled in the coach's program, including their medical file.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<GetCoachClientProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid coachId,
        Guid memberId,
        GetCoachClientProfileHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetCoachClientProfileQuery(coachId, memberId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
