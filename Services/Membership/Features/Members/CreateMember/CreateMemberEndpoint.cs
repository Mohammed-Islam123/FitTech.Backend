using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Wolverine;

namespace Membership.Features.Members.CreateMember;

public class CreateMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/members", Handle)
            .WithName("CreateMember")
            .WithDescription("Creates a new member with the provided details and profile picture.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                // Define the example data
                var exampleData = new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Sample Item",
                    ["isComplete"] = false
                };

                // Apply to Request (JSON or Multipart)
                var mediaType = "application/json"; // or "multipart/form-data"
                if (operation.RequestBody?.Content?.TryGetValue(mediaType, out var reqContent) == true)
                {
                    reqContent.Example = exampleData;
                }

                // Apply to Response
                if (operation.Responses is not null &&
                    operation.Responses.TryGetValue("200", out var response) &&
                    response.Content is not null &&
                    response.Content.TryGetValue("application/json", out var resContent))
                {
                    resContent.Example = exampleData;
                }

                return Task.CompletedTask;
            })
            // .RequireAuthorization("AdminOnly") // Temporarily commented if testing locally without full auth
            .DisableAntiforgery() // Needed for multipart/form-data with Minimal APIs if antiforgery is enabled
            .Produces<CreateMemberResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }


    private static async Task<IResult> Handle(
        [FromForm] CreateMemberRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<CreateMemberResponse>>(new CreateMemberCommand(request), ct);

        return result.Match(
            response => Results.Created($"/api/members/{response.MemberId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }


}
