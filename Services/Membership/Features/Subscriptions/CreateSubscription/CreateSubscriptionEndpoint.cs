using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;

namespace Membership.Features.Subscriptions.CreateSubscription;

public class CreateSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subscriptions", Handle)
            .WithName("CreateSubscription")
            .WithDescription("Creates a new subscription for a member. Restricted to Admin role.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["memberId"] = "550e8400-e29b-41d4-a716-446655440000",
                    ["planId"] = "660e8400-e29b-41d4-a716-446655440001",
                    ["paymentMethod"] = "Cash",
                    ["notes"] = "3-month membership"
                };

                var exampleResponse = new JsonObject
                {
                    ["subscriptionId"] = "770e8400-e29b-41d4-a716-446655440002",
                    ["paymentStatus"] = "Pending"
                };

                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                {
                    reqContent.Example = exampleRequest;
                }

                if (operation.Responses is not null &&
                    operation.Responses.TryGetValue("201", out var response) &&
                    response.Content is not null &&
                    response.Content.TryGetValue("application/json", out var resContent))
                {
                    resContent.Example = exampleResponse;
                }

                return Task.CompletedTask;
            })
            .RequireAuthorization("AdminOnly")
            .Produces<CreateSubscriptionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateSubscriptionRequest request,
        CreateSubscriptionHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(
            new CreateSubscriptionCommand(request), ct);

        return result.Match(
            response => Results.Created($"/api/subscriptions/{response.SubscriptionId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
