using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.RejectRenewal;

public class RejectRenewalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/subscriptions/renew/{requestId:guid}/reject", Handle)
            .WithName("RejectMembershipRenewal")
            .WithDescription("Admin rejects a renewal request. Notifies the member.")
            .RequireAuthorization("AdminOnly")
            .Produces<RejectRenewalResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid requestId,
        [FromBody] RejectRenewalRequest? body,
        RejectRenewalHandler handler,
        CancellationToken ct)
    {
        var request = new RejectRenewalRequest(requestId, body?.Reason);
        var result = await handler.Handle(new RejectRenewalCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
