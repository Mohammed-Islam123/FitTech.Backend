using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Members.UpdateMyProfile;

public class UpdateMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/me", Handle)
            .WithName("UpdateMyProfileViaMe")
            .DisableAntiforgery()
            .WithTags("Members")
            .WithDescription("Updates the authenticated member's profile. All fields are optional (PATCH semantics). Supports name, phone, gender, date of birth, profile picture, goals, medical restrictions, medical file, and password change via Identity service.")
            .RequireAuthorization("MemberOnly")
            .Produces<UpdateMyProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["memberId"] = Guid.NewGuid().ToString()
                };

                if (operation.Responses.TryGetValue("200", out var response) &&
                    response.Content.TryGetValue("application/json", out var content))
                {
                    content.Example = exampleResponse;
                }

                if (operation.RequestBody?.Content?.TryGetValue("multipart/form-data", out var reqContent) == true)
                {
                    reqContent.Example = new JsonObject
                    {
                        ["firstName"] = "Jane",
                        ["lastName"] = "Smith",
                        ["phoneNumber"] = "0987654321",
                        ["gender"] = 2,
                        ["dateOfBirth"] = "1995-05-05",
                        ["goals"] = "Gain muscle",
                        ["medicalRestrictions"] = "Back pain",
                        ["oldPassword"] = "OldP@ss123",
                        ["newPassword"] = "NewP@ss123"
                    };
                }

                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromForm] UpdateMyProfileRequest request,
        UpdateMyProfileHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(
            new UpdateMyProfileCommand(request), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
