using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Membership.Features.Members.UpdateMyProfile;

public class UpdateMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/me", Handle)
            .WithName("UpdateMyProfileViaMe")
            .DisableAntiforgery()
            .WithTags("Members")
            .WithDescription("Updates the authenticated member's profile: medical file, goals, and/or profile picture.")
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

                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromForm] UpdateMyProfileRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<UpdateMyProfileResponse>>(
            new UpdateMyProfileCommand(request), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
