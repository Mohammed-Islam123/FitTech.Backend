using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Payment.Shared;

namespace Payment.Features.Requests.RequestCoursePurchase;

public class RequestCoursePurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/requests/course-purchase", Handle)
            .WithName("RequestCoursePurchase")
            .WithTags("Requests")
            .WithDescription("Member submits a request to purchase a course via hand-to-hand payment.")
            .RequireAuthorization("MemberOnly")
            .Produces<RequestCoursePurchaseResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleRequest = new JsonObject
                {
                    ["courseId"] = Guid.NewGuid().ToString(),
                    ["amount"] = 3000m,
                    ["notes"] = "HIIT course payment"
                };
                if (operation.RequestBody?.Content?.TryGetValue("application/json", out var reqContent) == true)
                    reqContent.Example = exampleRequest;
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> Handle(
        [FromBody] RequestCoursePurchaseRequest request,
        RequestCoursePurchaseHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new RequestCoursePurchaseCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/requests/course-purchase/{response.RequestId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
