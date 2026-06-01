using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.OnlineRenewal;

public class OnlineRenewalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subscriptions/renew/online", Handle)
            .WithName("OnlineRenewal")
            .WithTags("Subscriptions")
            .WithDescription("Member submits an online (credit card) renewal. Payment is processed immediately and the subscription is extended automatically.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var example = new JsonObject
                {
                    ["subscriptionId"] = "550e8400-e29b-41d4-a716-446655440000",
                    ["amount"] = 5000m,
                    ["notes"] = "Online renewal"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = example;
                return Task.CompletedTask;
            })
            .RequireAuthorization()
            .Produces<OnlineRenewalResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        [FromBody] OnlineRenewalRequest request,
        OnlineRenewalHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new OnlineRenewalCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
