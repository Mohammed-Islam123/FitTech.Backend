using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.RequestPurchase;

public class RequestPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subscriptions/purchase", Handle)
            .WithName("RequestPlanPurchase")
            .WithTags("Subscriptions")
            .WithDescription("Member submits a cash purchase request for a new plan. Admin must approve.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var example = new JsonObject
                {
                    ["planId"] = "660e8400-e29b-41d4-a716-446655440001",
                    ["notes"] = "Will pay cash at the front desk"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = example;
                return Task.CompletedTask;
            })
            .RequireAuthorization()
            .Produces<RequestPurchaseResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [FromBody] RequestPurchaseRequest request,
        RequestPurchaseHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new RequestPurchaseCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/subscriptions/purchase/{response.RequestId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
