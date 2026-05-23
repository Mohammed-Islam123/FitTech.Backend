using Carter;
using ErrorOr;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json.Nodes;
using Wolverine;

namespace Membership.Features.Members.GetActiveSubscription;

public class GetActiveSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/members/active-subscription", Handle)
            .WithName("GetActiveSubscription")
            .RequireAuthorization()
            .Produces<GetActiveSubscriptionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Members")
            .WithDescription("Retrieves the currently active subscription for the authenticated member or a specific member (Admin only).")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["subscriptionId"] = Guid.NewGuid().ToString(),
                    ["planName"] = "Monthly Fitness",
                    ["price"] = 39.99m,
                    ["startOnUTC"] = DateTime.UtcNow.ToString("O"),
                    ["endOnUTC"] = DateTime.UtcNow.AddMonths(1).ToString("O"),
                    ["status"] = "Active",
                    ["remainingSessions"] = null,
                    ["pausedUntil"] = null
                };

                if (operation.Responses.TryGetValue("200", out var response) &&
                    response.Content.TryGetValue("application/json", out var content))
                {
                    content.Example = exampleResponse;
                }

                return Task.CompletedTask;
            });
    }

    /// <description>
    /// Fetches the current active subscription.
    /// </description>
    /// <example>
    /// GET /api/members/active-subscription?memberId=01948372-5a3d-7b2e-a1c4-83920174be62
    /// </example>
    private static async Task<IResult> Handle(
        Guid? memberId,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<GetActiveSubscriptionResponse>>(
            new GetActiveSubscriptionQuery(memberId), 
            ct);

        return result.Match(
            response => Results.Ok(response),
            errors => MapErrorsToResult(errors));
    }

    private static IResult MapErrorsToResult(List<Error> errors)
    {
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}
