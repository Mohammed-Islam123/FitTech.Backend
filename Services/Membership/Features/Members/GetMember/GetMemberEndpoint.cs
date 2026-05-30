using Carter;
using ErrorOr;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json.Nodes;
using Membership.Shared;
namespace Membership.Features.Members.GetMember;

public class GetMemberEndpoint : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/members/{id:guid}", Handle)
            .WithName("GetMember")
            .RequireAuthorization(r => r.RequireRole("Admin", "Coach"))
            .Produces<GetMemberResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Members")
            .WithDescription("Retrieves a specific member's details by ID, including active subscription, health profile, and NFC card information.")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var exampleResponse = EndpointExample;

                if (operation.Responses is not null &&
                    operation.Responses.TryGetValue("200", out var response) &&
                    response.Content is not null &&
                    response.Content.TryGetValue("application/json", out var content))
                {
                    content.Example = exampleResponse;
                }

                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Fetches a member's full profile. Requires Admin or Coach role.
    /// </summary>
    /// <example>
    /// <code>
    /// GET /api/members/01948372-5a3d-7b2e-a1c4-83920174be62
    /// </code>
    /// </example>

    private static async Task<IResult> Handle(
        Guid id,
        GetMemberHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetMemberQuery(id), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }



    private static readonly JsonNode? EndpointExample = new JsonObject
    {
        ["memberId"] = Guid.NewGuid().ToString(),
        ["userId"] = Guid.NewGuid().ToString(),
        ["firstName"] = "John",
        ["lastName"] = "Doe",
        ["joinDate"] = DateTime.UtcNow.ToString("O"),
        ["status"] = "Active",
        ["noShowWarningCount"] = 0,
        ["activeSubscription"] = new JsonObject
        {
            ["subscriptionId"] = Guid.NewGuid().ToString(),
            ["planName"] = "Premium Monthly",
            ["startOnUTC"] = DateTime.UtcNow.ToString("O"),
            ["endOnUTC"] = DateTime.UtcNow.AddMonths(1).ToString("O"),
            ["remainingSessions"] = 12,
            ["status"] = "Active"
        },
        ["healthProfile"] = new JsonObject
        {
            ["objectives"] = "Weight loss and muscle gain",
            ["medicalRestrictions"] = "None",
            ["lastUpdatedAt"] = DateTime.UtcNow.ToString("O")
        },
        ["activeCardUid"] = "NF-12345678"
    };

}
