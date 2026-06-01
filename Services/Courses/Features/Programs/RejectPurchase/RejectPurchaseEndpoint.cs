using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.RejectPurchase;

public class RejectPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/programs/purchase/{requestId:guid}/reject", Handle)
            .WithName("RejectCoursePurchase")
            .WithTags("Programs")
            .WithDescription("Admin rejects a course purchase request. Notifies the member.")
            .RequireAuthorization("AdminOnly")
            .Produces<RejectPurchaseResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid requestId,
        [FromBody] string? reason,
        RejectPurchaseHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new RejectPurchaseCommand(requestId, reason), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
