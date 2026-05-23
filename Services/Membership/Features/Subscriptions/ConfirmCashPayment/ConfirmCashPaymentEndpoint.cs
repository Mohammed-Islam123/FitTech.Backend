using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Membership.Shared;
using Wolverine;

namespace Membership.Features.Subscriptions.ConfirmCashPayment;

public class ConfirmCashPaymentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/subscriptions/confirm-payment", Handle)
            .WithName("ConfirmCashPayment")
            .WithDescription("Confirms a cash payment for a subscription. Restricted to Admin role.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["subscriptionId"] = "770e8400-e29b-41d4-a716-446655440002",
                    ["amountReceived"] = 5000m,
                    ["paymentMethod"] = "Cash",
                    ["notes"] = "Cash payment received at counter"
                };

                var exampleResponse = new JsonObject
                {
                    ["subscriptionId"] = "770e8400-e29b-41d4-a716-446655440002",
                    ["paymentId"] = "880e8400-e29b-41d4-a716-446655440003",
                    ["paymentStatus"] = "Paid"
                };

                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                {
                    reqContent.Example = exampleRequest;
                }

                if (operation.Responses is not null &&
                    operation.Responses.TryGetValue("200", out var response) &&
                    response.Content is not null &&
                    response.Content.TryGetValue("application/json", out var resContent))
                {
                    resContent.Example = exampleResponse;
                }

                return Task.CompletedTask;
            })
            .RequireAuthorization("AdminOnly")
            .Produces<ConfirmCashPaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        [FromBody] ConfirmCashPaymentRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<ConfirmCashPaymentResponse>>(
            new ConfirmCashPaymentCommand(request), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
