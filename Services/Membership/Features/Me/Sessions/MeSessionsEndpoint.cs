using Carter;
using Membership.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Me.Sessions;

public class MeSessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/sessions", Handle)
            .WithName("MeSessions")
            .WithTags("Me")
            .WithDescription("Returns the authenticated member's gym entry/exit session history.")
            .RequireAuthorization("MemberOnly")
            .Produces<List<MeSessionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        MeSessionsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
