using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Payment.Shared;

namespace Payment.Features.Payments.Webhook;

/// <description>
/// Endpoint for payment gateway webhooks. Reads the raw body and signature
/// header for validation. This is called by external services (Stripe, etc.).
/// </description>
public class WebhookEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/webhooks/{provider}", Handle)
            .WithName("PaymentWebhook")
            .WithDescription("Receives payment gateway webhook callbacks. Validates signature and confirms payment.")
            .AllowAnonymous() // Webhook endpoints are called by external gateways
            .Produces<WebhookResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string provider,
        HttpContext httpContext,
        WebhookHandler handler,
        CancellationToken ct)
    {
        var signatureHeader = httpContext.Request.Headers["Stripe-Signature-Webhook"].FirstOrDefault()
            ?? httpContext.Request.Headers["Webhook-Signature"].FirstOrDefault()
            ?? string.Empty;

        using var reader = new StreamReader(httpContext.Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        var request = new WebhookRequest(provider, payload, signatureHeader);
        var result = await handler.Handle(new WebhookCommand(request), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
