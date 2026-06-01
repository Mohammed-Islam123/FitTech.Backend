using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.AcceptPurchase;

public class AcceptPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/programs/purchase/{requestId:guid}/accept", Handle)
            .WithName("AcceptCoursePurchase")
            .WithTags("Programs")
            .WithDescription("Admin accepts a cash course purchase request. Records payment and creates enrollment.")
            .RequireAuthorization("AdminOnly")
            .Produces<AcceptPurchaseResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid requestId,
        [FromBody] string? notes,
        AcceptPurchaseHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new AcceptPurchaseCommand(requestId, notes), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
