using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Courses.Features.Programs.PurchaseCash;

public class PurchaseCashEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/programs/{programId:guid}/purchase/cash", Handle)
            .WithName("PurchaseProgramCash")
            .WithTags("Programs")
            .WithDescription("Member submits a cash purchase request for a program. Admin must approve.")
            .RequireAuthorization("MemberOnly")
            .Produces<PurchaseCashResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        [FromBody] PurchaseCashRequest request,
        PurchaseCashHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new PurchaseCashCommand(programId, request), ct);
        return result.Match(
            response => Results.Created($"/api/programs/purchase/{response.RequestId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
