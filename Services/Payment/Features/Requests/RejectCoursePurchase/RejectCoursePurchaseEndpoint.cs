using Carter;
using ErrorOr;
using Payment.Shared;

namespace Payment.Features.Requests.RejectCoursePurchase;

public class RejectCoursePurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/requests/course-purchase/{requestId:guid}/reject", Handle)
            .WithName("RejectCoursePurchase")
            .WithTags("Requests")
            .WithDescription("Admin rejects a course purchase request.")
            .RequireAuthorization("AdminOnly")
            .Produces<AcceptRequestResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(Guid requestId, RejectCoursePurchaseHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(new RejectCoursePurchaseCommand(requestId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
