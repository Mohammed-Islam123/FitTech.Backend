using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Membership.Features.Courses.GetCourseDetail;

public class GetCourseDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/courses/{courseId:guid}", Handle)
            .WithName("GetCourseDetail")
            .WithTags("Courses")
            .WithDescription("Returns the full details of a specific course.")
            .RequireAuthorization("MemberOnly")
            .Produces<GetCourseDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["id"] = Guid.NewGuid().ToString(),
                    ["name"] = "HIIT Training",
                    ["price"] = 1500m,
                    ["description"] = "High intensity interval training for all levels.",
                    ["spotsLeft"] = 5,
                    ["capacity"] = 20,
                    ["sessions"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["id"] = Guid.NewGuid().ToString(),
                            ["day"] = "Monday",
                            ["startTime"] = "08:00",
                            ["endTime"] = "09:00",
                            ["description"] = "Morning session"
                        }
                    }
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
        Guid courseId,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<GetCourseDetailResponse>>(
            new GetCourseDetailQuery(courseId), ct);

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
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}
