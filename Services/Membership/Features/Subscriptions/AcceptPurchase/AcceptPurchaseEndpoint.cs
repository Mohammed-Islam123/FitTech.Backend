using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.AcceptPurchase;

public class AcceptPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/subscriptions/purchase/{requestId:guid}/accept", Handle)
            .WithName("AcceptPlanPurchase")
            .WithTags("Subscriptions")
            .WithDescription("Admin accepts a cash purchase request. Creates the subscription, records payment, and activates the plan.")
            .RequireAuthorization("AdminOnly")
            .Produces<AcceptPurchaseResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid requestId,
        [FromBody] AcceptPurchaseRequest? body,
        AcceptPurchaseHandler handler,
        CancellationToken ct)
    {
        var request = new AcceptPurchaseRequest(requestId, body?.Notes);
        var result = await handler.Handle(new AcceptPurchaseCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
