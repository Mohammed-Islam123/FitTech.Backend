using System.Text.Json.Nodes;
using Activity.Shared;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Activity.Features.EntryExit.ManualExit;

public class ManualExitEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/activity/entry-exit/manual/exit", Handle)
            .WithName("ManualExit")
            .WithTags("EntryExit")
            .WithDescription("Manually marks a member as exited. Takes member ID and optionally a course ID.")
            .RequireAuthorization("AdminOnly")
            .Produces<ManualExitResponse>(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var c) == true)
                    c.Example = new JsonObject { ["memberId"] = Guid.NewGuid().ToString(), ["courseId"] = null };
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] ManualExitRequest request, IMessageBus messageBus, CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<ManualExitResponse>>(
            new ManualExitCommand(request), ct);
        return result.Match(r => Results.Ok(r), errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
