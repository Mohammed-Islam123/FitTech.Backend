using Carter;
using ErrorOr;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json.Nodes;
using Wolverine;

namespace Membership.Features.Members.ListMembers;

public class ListMembersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/members", Handle)
            .WithName("ListMembers")
            .RequireAuthorization(r => r.RequireRole("Admin", "Coach"))
            .Produces<ListMembersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithTags("Members")
            .WithDescription("Lists gym members with pagination, keyword search, and status filtering. Restricted to Admin and Coach roles.")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var exampleItem = new JsonObject
                {
                    ["memberId"] = Guid.NewGuid().ToString(),
                    ["firstName"] = "Jane",
                    ["lastName"] = "Smith",
                    ["status"] = "Active",
                    ["joinDate"] = DateTime.UtcNow.AddMonths(-3).ToString("O"),
                    ["activePlanName"] = "Standard Monthly"
                };

                var exampleResponse = new JsonObject
                {
                    ["items"] = new JsonArray { exampleItem },
                    ["totalCount"] = 1,
                    ["page"] = 1,
                    ["pageSize"] = 10,
                    ["totalPages"] = 1
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
    /// Returns a paginated list of members. Can filter by status and search by name.
    /// </description>
    /// <example>
    /// GET /api/members?page=1&pageSize=20&search=doe&status=Active
    /// </example>
    private static async Task<IResult> Handle(
        [AsParameters] ListMembersRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<ListMembersResponse>>(new ListMembersQuery(request), ct);

        return result.Match(
            response => Results.Ok(response),
            errors => MapErrorsToResult(errors));
    }

    private static IResult MapErrorsToResult(List<Error> errors)
    {
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}
