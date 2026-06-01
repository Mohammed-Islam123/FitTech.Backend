using Carter;
using ErrorOr;
using Membership.Shared;

namespace Membership.Features.Members.GetMemberByCard;

public class GetMemberByCardEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/members/by-card/{cardUid}", Handle)
            .WithName("GetMemberByCard")
            .RequireAuthorization(r => r.RequireRole("Admin", "Coach"))
            .Produces<GetMemberByCardResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Members")
            .WithDescription("Resolves a member from an active NFC card UID. Used by the Activity service for NFC entry/exit flow.");
    }

    private static async Task<IResult> Handle(
        string cardUid,
        GetMemberByCardHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMemberByCardQuery(cardUid), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
