using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Payment.Shared;

namespace Payment.Features.Requests.RequestMembershipRenewal;

public class RequestMembershipRenewalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/requests/membership-renewal", Handle)
            .WithName("RequestMembershipRenewal")
            .WithTags("Requests")
            .WithDescription("Member submits a request to renew their membership via hand-to-hand payment.")
            .RequireAuthorization("MemberOnly")
            .Produces<RequestMembershipRenewalResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["subscriptionId"] = Guid.NewGuid().ToString(),
                    ["amount"] = 5000m,
                    ["notes"] = "Monthly renewal payment"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = exampleRequest;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] RequestMembershipRenewalRequest request,
        RequestMembershipRenewalHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new RequestMembershipRenewalCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/requests/membership-renewal/{response.RequestId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
