using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.AcceptRenewal;

public class AcceptRenewalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/subscriptions/renew/{requestId:guid}/accept", Handle)
            .WithName("AcceptMembershipRenewal")
            .WithDescription("Admin accepts a renewal request. Creates payment record and activates extended subscription.")
            .RequireAuthorization("AdminOnly")
            .Produces<AcceptRenewalResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid requestId,
        [FromBody] AcceptRenewalRequest? body,
        AcceptRenewalHandler handler,
        CancellationToken ct)
    {
        var request = new AcceptRenewalRequest(requestId, body?.Notes);
        var result = await handler.Handle(new AcceptRenewalCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
