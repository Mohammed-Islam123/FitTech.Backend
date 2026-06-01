using Carter;
using ErrorOr;
using Payment.Shared;

namespace Payment.Features.Payments.ConfirmPayment;

public class ConfirmPaymentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/{paymentId:guid}/confirm", Handle)
            .WithName("ConfirmPayment")
            .WithDescription("Confirms a pending online payment (simulates gateway webhook callback). Called internally by the Membership service.")
            .RequireAuthorization()
            .Produces<ConfirmPaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid paymentId,
        ConfirmPaymentHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new ConfirmPaymentCommand(paymentId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
