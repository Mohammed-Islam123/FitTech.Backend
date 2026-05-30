using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Features.Members.GetActiveSubscription;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Members.GetMySubscription;

public class GetMySubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/subscription", Handle)
            .WithName("GetMySubscription")
            .WithTags("Members")
            .WithDescription("Returns the current subscription details for the authenticated member.")
            .RequireAuthorization("MemberOnly")
            .Produces<GetActiveSubscriptionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
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

    private static async Task<IResult> Handle(
        GetActiveSubscriptionHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(
            new GetActiveSubscriptionQuery(null), ct);

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
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}
