using System.Text.Json.Nodes;
using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Courses.Features.Programs.RejectProgram;

public class RejectProgramEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/programs/requests/{programId:guid}/reject", Handle)
            .WithName("RejectProgram")
            .WithTags("Programs")
            .WithDescription("Admin rejects a program creation request.")
            .RequireAuthorization("AdminOnly")
            .Produces<RejectProgramResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["programId"] = Guid.NewGuid().ToString(),
                    ["status"] = "Rejected"
                };
                if (operation.Responses.TryGetValue("200", out var resp) &&
                    resp.Content.TryGetValue("application/json", out var resContent))
                    resContent.Example = exampleResponse;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        Guid programId,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<RejectProgramResponse>>(
            new RejectProgramCommand(programId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
