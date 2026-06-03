using System.Text.Json.Nodes;
using Carter;
using Membership.Shared;

namespace Membership.Features.Members.TrackMemberEntry;

public class TrackMemberEntryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/members/track-entry", Handle)
            .WithName("TrackMemberEntry")
            .WithTags("Members")
            .WithDescription("Validates member eligibility and records a gym entry. Decrements remaining sessions for session-limited plans. Auto-expires subscription when sessions reach zero.")
            .RequireAuthorization(r => r.RequireRole("Admin", "Coach"))
            .Produces<TrackMemberEntryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var c) == true)
                    c.Example = new JsonObject { ["memberId"] = "00000000-0000-0000-0000-000000000000" };
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        TrackMemberEntryRequest request,
        TrackMemberEntryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new TrackMemberEntryCommand(request), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
