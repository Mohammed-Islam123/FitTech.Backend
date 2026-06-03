using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.RejectPurchase;

public class RejectPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/subscriptions/purchase/{requestId:guid}/reject", Handle)
            .WithName("RejectPlanPurchase")
            .WithTags("Subscriptions")
            .WithDescription("Admin rejects a cash purchase request. Notifies the member.")
            .RequireAuthorization("AdminOnly")
            .Produces<RejectPurchaseResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid requestId,
        [FromBody] RejectPurchaseRequest? body,
        RejectPurchaseHandler handler,
        CancellationToken ct)
    {
        var request = new RejectPurchaseRequest(requestId, body?.Reason);
        var result = await handler.Handle(new RejectPurchaseCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
