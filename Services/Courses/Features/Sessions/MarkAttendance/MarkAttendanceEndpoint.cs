using System.Text.Json.Nodes;
using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Sessions.MarkAttendance;

public class MarkAttendanceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sessions/{sessionId:guid}/attendance", Handle)
            .WithName("MarkAttendance")
            .WithTags("Sessions")
            .WithDescription("Coach marks attendance for a specific session. For each member specifies present or absent.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<MarkAttendanceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["attendance"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["memberId"] = Guid.NewGuid().ToString(),
                            ["status"] = "Present"
                        },
                        new JsonObject
                        {
                            ["memberId"] = Guid.NewGuid().ToString(),
                            ["status"] = "Absent"
                        }
                    }
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = exampleRequest;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        Guid sessionId,
        [FromBody] MarkAttendanceRequest request,
        MarkAttendanceHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new MarkAttendanceCommand(sessionId, request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
