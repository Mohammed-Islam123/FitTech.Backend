using Carter;
using ErrorOr;
using Payment.Shared;
using Wolverine;

namespace Payment.Features.Requests.ListMembershipRenewalRequests;

public class ListMembershipRenewalRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/requests/membership-renewal", Handle)
            .WithName("ListMembershipRenewalRequests")
            .WithTags("Requests")
            .WithDescription("Returns the list of pending membership renewal requests.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<ListRequestResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(IMessageBus messageBus, CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<List<ListRequestResponse>>>(
            new ListMembershipRenewalRequestsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
