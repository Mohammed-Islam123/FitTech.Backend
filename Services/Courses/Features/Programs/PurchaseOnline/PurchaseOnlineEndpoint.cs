using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.PurchaseOnline;

public class PurchaseOnlineEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/programs/{programId:guid}/purchase/online", Handle)
            .WithName("PurchaseProgramOnline")
            .WithTags("Programs")
            .WithDescription("Member purchases a program online (credit card). Payment is processed immediately and enrollment is created automatically.")
            .RequireAuthorization("MemberOnly")
            .Produces<PurchaseOnlineResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        [FromBody] PurchaseOnlineRequest request,
        PurchaseOnlineHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new PurchaseOnlineCommand(programId, request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
