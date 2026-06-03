using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.OnlinePurchase;

public class OnlinePurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subscriptions/purchase/online", Handle)
            .WithName("OnlinePurchase")
            .WithTags("Subscriptions")
            .WithDescription("Member purchases a new plan online (credit card). Payment is processed immediately and the subscription is activated automatically.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var example = new JsonObject
                {
                    ["planId"] = "660e8400-e29b-41d4-a716-446655440001",
                    ["notes"] = "Purchasing new monthly plan"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = example;
                return Task.CompletedTask;
            })
            .RequireAuthorization()
            .Produces<OnlinePurchaseResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        [FromBody] OnlinePurchaseRequest request,
        OnlinePurchaseHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new OnlinePurchaseCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
