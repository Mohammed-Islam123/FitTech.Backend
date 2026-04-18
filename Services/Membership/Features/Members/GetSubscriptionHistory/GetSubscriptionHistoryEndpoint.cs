using Carter;
using ErrorOr;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json.Nodes;
using Wolverine;

namespace Membership.Features.Members.GetSubscriptionHistory;

public class GetSubscriptionHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/members/{id:guid}/subscriptions", Handle)
            .WithName("GetSubscriptionHistory")
            .RequireAuthorization() // Basic auth required, refined in handler
            .Produces<SubscriptionHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Members")
            .WithDescription("Retrieves the full subscription history for a specific member. Admins and Coaches can see any member; Members can only see their own history.")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var exampleItem = new JsonObject
                {
                    ["subscriptionId"] = Guid.NewGuid().ToString(),
                    ["planName"] = "Premium Monthly",
                    ["startOnUTC"] = DateTime.UtcNow.AddMonths(-1).ToString("O"),
                    ["endOnUTC"] = DateTime.UtcNow.ToString("O"),
                    ["price"] = 49.99m,
                    ["status"] = "Expired",
                    ["remainingSessions"] = 0,
                    ["cancelledAt"] = null
                };

                var exampleResponse = new JsonObject
                {
                    ["subscriptions"] = new JsonArray { exampleItem }
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
    /// Fetches all subscriptions for a member.
    /// </description>
    /// <example>
    /// GET /api/members/01948372-5a3d-7b2e-a1c4-83920174be62/subscriptions
    /// </example>
    private static async Task<IResult> Handle(
        Guid id,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<SubscriptionHistoryResponse>>(new GetSubscriptionHistoryQuery(id), ct);

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
