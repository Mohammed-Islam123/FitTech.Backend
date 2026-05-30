using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Payment.Shared;

namespace Payment.Features.Payments.CreatePayment;

public class CreatePaymentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments", Handle)
            .WithName("CreatePayment")
            .WithDescription("Creates a new payment record. Restricted to Admin and Member roles.")
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["userId"] = "550e8400-e29b-41d4-a716-446655440000",
                    ["amount"] = 5000m,
                    ["paymentMethod"] = "Cash",
                    ["paymentType"] = "Subscription",
                    ["referenceId"] = "660e8400-e29b-41d4-a716-446655440001",
                    ["notes"] = "Monthly subscription payment"
                };

                var exampleResponse = new JsonObject
                {
                    ["paymentId"] = "770e8400-e29b-41d4-a716-446655440002"
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
            .RequireAuthorization()
            .Produces<CreatePaymentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePaymentRequest request,
        CreatePaymentHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreatePaymentCommand(request), ct);

        return result.Match(
            response => Results.Created($"/api/payments/{response.PaymentId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
