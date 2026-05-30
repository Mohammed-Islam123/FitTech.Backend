using Carter;
using ErrorOr;
using Payment.Shared;

namespace Payment.Features.Requests.ListCoursePurchaseRequests;

public class ListCoursePurchaseRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/requests/course-purchase", Handle)
            .WithName("ListCoursePurchaseRequests")
            .WithTags("Requests")
            .WithDescription("Returns the list of pending course purchase requests.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<ListMembershipRenewalRequests.ListRequestResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(ListCoursePurchaseRequestsHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(new ListCoursePurchaseRequestsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
