using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Members.SuspendMember;

public class SuspendMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/members/{memberId:guid}/suspend", Handle)
            .WithName("SuspendMember")
            .WithTags("Members")
            .WithDescription("Suspends a member account. Restricted to Administrators.")
            .RequireAuthorization("AdminOnly")
            .Produces<SuspendMemberResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["memberId"] = Guid.NewGuid().ToString(),
                    ["status"] = "Suspended"
                };

                if (operation.Responses.TryGetValue("200", out var response) &&
                    response.Content.TryGetValue("application/json", out var content))
                {
                    content.Example = exampleResponse;
                }

                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        Guid memberId,
        SuspendMemberHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(
            new SuspendMemberCommand(memberId), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
