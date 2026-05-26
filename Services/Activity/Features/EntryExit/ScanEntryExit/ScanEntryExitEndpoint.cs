using System.Text.Json.Nodes;
using Activity.Shared;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Activity.Features.EntryExit.ScanEntryExit;

public class ScanEntryExitEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/activity/entry-exit/scan", Handle)
            .WithName("ScanEntryExit")
            .WithTags("EntryExit")
            .WithDescription("Takes an NFC card ID, verifies member eligibility, and auto-logs entry or exit.")
            .RequireAuthorization("AdminOnly")
            .Produces<ScanEntryExitResponse>(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var c) == true)
                    c.Example = new JsonObject { ["cardUid"] = "A1B2C3D4" };
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] ScanEntryExitRequest request, IMessageBus messageBus, CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<ScanEntryExitResponse>>(
            new ScanEntryExitCommand(request), ct);
        return result.Match(r => Results.Ok(r), errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
