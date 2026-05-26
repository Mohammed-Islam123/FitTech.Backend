using System.Text.Json.Nodes;
using Activity.Shared;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Activity.Features.EntryExit.ManualEnter;

public class ManualEnterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/activity/entry-exit/manual/enter", Handle)
            .WithName("ManualEnter")
            .WithTags("EntryExit")
            .WithDescription("Manually marks a member as entered. Takes member ID and optionally a course ID.")
            .RequireAuthorization("AdminOnly")
            .Produces<ManualEnterResponse>(StatusCodes.Status201Created)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var c) == true)
                    c.Example = new JsonObject { ["memberId"] = Guid.NewGuid().ToString(), ["courseId"] = null };
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] ManualEnterRequest request, IMessageBus messageBus, CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<ManualEnterResponse>>(
            new ManualEnterCommand(request), ct);
        return result.Match(
            r => Results.Created($"/api/activity/sessions/{r.SessionId}", r),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
