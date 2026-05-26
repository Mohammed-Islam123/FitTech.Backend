using Carter;
using ErrorOr;
using Payment.Shared;
using Wolverine;

namespace Payment.Features.Payments.ListPayments;

public class ListPaymentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments", Handle)
            .WithName("ListPayments")
            .WithTags("Payments")
            .WithDescription("Returns a list of all payments with member details. Restricted to Administrators.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<ListPaymentsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<List<ListPaymentsResponse>>>(
            new ListPaymentsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
