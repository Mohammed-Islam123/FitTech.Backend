using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Coaches.GetMyPurchaseRequests;

public class GetMyPurchaseRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/me/purchase-requests", Handle)
            .WithName("GetMyPurchaseRequests")
            .WithTags("Coaches")
            .WithDescription("Returns all course purchase requests for programs belonging to the currently authenticated coach, including all statuses (Pending, Accepted, Rejected). Coach is resolved from the JWT token.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetMyPurchaseRequestsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        GetMyPurchaseRequestsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMyPurchaseRequestsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
