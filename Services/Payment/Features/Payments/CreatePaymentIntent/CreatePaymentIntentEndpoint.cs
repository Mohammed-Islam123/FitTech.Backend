using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Payment.Shared;

namespace Payment.Features.Payments.CreatePaymentIntent;

public class CreatePaymentIntentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/intent", Handle)
            .WithName("CreatePaymentIntent")
            .WithDescription("Creates a payment intent for online/card payments. Returns a client secret for frontend checkout.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["userId"] = "550e8400-e29b-41d4-a716-446655440000",
                    ["amount"] = 5000m,
                    ["paymentMethod"] = "CreditCard",
                    ["paymentType"] = "Subscription",
                    ["referenceId"] = "660e8400-e29b-41d4-a716-446655440001",
                    ["notes"] = "Monthly subscription"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                {
                    reqContent.Example = exampleRequest;
                }
                return Task.CompletedTask;
            })
            .RequireAuthorization()
            .Produces<CreatePaymentIntentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePaymentIntentRequest request,
        CreatePaymentIntentHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreatePaymentIntentCommand(request), ct);

        return result.Match(
            response => Results.Created($"/api/payments/{response.PaymentId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
