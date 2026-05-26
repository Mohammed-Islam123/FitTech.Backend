using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Membership.Features.Courses.GetAvailableCourses;

public class GetAvailableCoursesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/courses/available", Handle)
            .WithName("GetAvailableCourses")
            .WithTags("Courses")
            .WithDescription("Returns the list of courses the authenticated member is not enrolled in.")
            .RequireAuthorization("MemberOnly")
            .Produces<List<GetAvailableCoursesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonArray
                {
                    new JsonObject
                    {
                        ["id"] = Guid.NewGuid().ToString(),
                        ["name"] = "HIIT Training",
                        ["imageUrl"] = "http://example.com/hiit.jpg",
                        ["price"] = 1500m,
                        ["coachName"] = "Coach Ahmed"
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
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<List<GetAvailableCoursesResponse>>>(
            new GetAvailableCoursesQuery(Guid.Empty), ct);

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
