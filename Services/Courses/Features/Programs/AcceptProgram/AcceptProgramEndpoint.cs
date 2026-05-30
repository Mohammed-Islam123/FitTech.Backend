using System.Text.Json.Nodes;
using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.AcceptProgram;

public class AcceptProgramEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/programs/requests/{programId:guid}/accept", Handle)
            .WithName("AcceptProgram")
            .WithTags("Programs")
            .WithDescription("Admin accepts a program creation request. Triggers session generation between start and end dates.")
            .RequireAuthorization("AdminOnly")
            .Produces<AcceptProgramResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["programId"] = Guid.NewGuid().ToString(),
                    ["status"] = "Accepted"
                };
                if (operation.Responses.TryGetValue("200", out var resp) &&
                    resp.Content.TryGetValue("application/json", out var resContent))
                    resContent.Example = exampleResponse;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        Guid programId,
        AcceptProgramHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new AcceptProgramCommand(programId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
