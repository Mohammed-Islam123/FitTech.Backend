using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.RequestRenewal;

public class RequestRenewalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subscriptions/renew", Handle)
            .WithName("RequestMembershipRenewal")
            .WithDescription("Member submits a renewal request for an existing subscription. Admin must approve.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var example = new JsonObject
                {
                    ["subscriptionId"] = "550e8400-e29b-41d4-a716-446655440000",
                    ["amount"] = 5000m,
                    ["notes"] = "Requesting renewal for next month"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = example;
                return Task.CompletedTask;
            })
            .RequireAuthorization()
            .Produces<RequestRenewalResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [FromBody] RequestRenewalRequest request,
        RequestRenewalHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new RequestRenewalCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/subscriptions/renew/{response.RequestId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
