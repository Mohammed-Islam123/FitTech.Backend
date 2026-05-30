using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Members.DeleteMember;

public class DeleteMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/members/{id:guid}", Handle)
            .WithName("DeleteMember")
            .WithTags("Members")
            .WithDescription("Soft-deletes (deactivates) a gym member. Restricted to Administrators.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        DeleteMemberHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new DeleteMemberCommand(id), ct);
        return result.Match(
            _ => Results.NoContent(),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
