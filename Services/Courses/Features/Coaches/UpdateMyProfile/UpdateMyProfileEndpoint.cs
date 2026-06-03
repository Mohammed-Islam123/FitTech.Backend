using Carter;
using Courses.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.UpdateMyProfile;

public class UpdateMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/coaches/me", Handle)
            .WithName("UpdateMyProfile")
            .WithTags("Coaches")
            .WithDescription("Updates the authenticated coach's profile fields: Bio (max 2000 chars), Specialties (max 500 chars), and ProfilePhotoUrl (max 500 chars). Coach is resolved from the JWT token.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<UpdateMyProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        UpdateMyProfileRequest request,
        UpdateMyProfileValidator validator,
        UpdateMyProfileHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(
            new UpdateMyProfileCommand(request), ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await handler.Handle(new UpdateMyProfileCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
