using System.Text.Json.Nodes;
using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.CreateCoach;

public class CreateCoachEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/coaches", Handle)
            .WithName("CreateCoach")
            .WithTags("Coaches")
            .WithDescription("Creates a new coach. Registers the user in Identity and creates a coach profile. Restricted to Administrators.")
            .RequireAuthorization("AdminOnly")
            .Produces<CreateCoachResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["firstName"] = "Ahmed",
                    ["lastName"] = "Benali",
                    ["email"] = "ahmed.benali@fittech.dz",
                    ["phoneNumber"] = "+213-555-789012",
                    ["dateOfBirth"] = "1990-05-15",
                    ["gender"] = "Male",
                    ["bio"] = "Certified personal trainer with 5 years experience."
                };
                var exampleResponse = new JsonObject
                {
                    ["coachId"] = Guid.NewGuid().ToString()
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = exampleRequest;
                if (operation.Responses.TryGetValue("201", out var resp) &&
                    resp.Content.TryGetValue("application/json", out var resContent))
                    resContent.Example = exampleResponse;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateCoachRequest request,
        CreateCoachHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreateCoachCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/coaches/{response.CoachId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
