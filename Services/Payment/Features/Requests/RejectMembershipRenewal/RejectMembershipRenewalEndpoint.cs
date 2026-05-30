using Carter;
using ErrorOr;
using Payment.Shared;

namespace Payment.Features.Requests.RejectMembershipRenewal;

public class RejectMembershipRenewalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/requests/membership-renewal/{requestId:guid}/reject", Handle)
            .WithName("RejectMembershipRenewal")
            .WithTags("Requests")
            .WithDescription("Admin rejects a membership renewal request.")
            .RequireAuthorization("AdminOnly")
            .Produces<AcceptRequestResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(Guid requestId, RejectMembershipRenewalHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(new RejectMembershipRenewalCommand(requestId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
