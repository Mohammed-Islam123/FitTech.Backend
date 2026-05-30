using System.Text.Json.Nodes;
using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.CreateProgram;

public class CreateProgramEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/programs", Handle)
            .WithName("CreateProgram")
            .WithTags("Programs")
            .WithDescription("Coach creates a new program and submits it for admin approval.")
            .RequireAuthorization("CoachOnly")
            .Produces<CreateProgramResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["name"] = "HIIT Training",
                    ["description"] = "High intensity interval training for all levels.",
                    ["level"] = "Intermediate",
                    ["exerciseType"] = "Cardio",
                    ["durationMinutes"] = 60,
                    ["startDate"] = "2026-06-01",
                    ["endDate"] = "2026-08-31",
                    ["totalPrice"] = 15000m,
                    ["maxParticipants"] = 20,
                    ["pictureUrl"] = "http://example.com/hiit.jpg",
                    ["timeSlots"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["day"] = "Monday",
                            ["startTime"] = "08:00",
                            ["endTime"] = "09:00",
                            ["description"] = "Morning session"
                        }
                    }
                };
                var exampleResponse = new JsonObject { ["programId"] = Guid.NewGuid().ToString() };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = exampleRequest;
                if (operation.Responses.TryGetValue("201", out var resp) &&
                    resp.Content.TryGetValue("application/json", out var resContent))
                    resContent.Example = exampleResponse;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateProgramRequest request,
        CreateProgramHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreateProgramCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/programs/{response.ProgramId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
