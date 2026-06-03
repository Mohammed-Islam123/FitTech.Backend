using Carter;
using Courses.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetMyProfile;

public class GetMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/me", Handle)
            .WithName("GetMyProfile")
            .WithTags("Coaches")
            .WithDescription("Returns the full profile of the currently authenticated coach, including email and phone number. Coach is resolved from the JWT token.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<GetMyProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        GetMyProfileHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
