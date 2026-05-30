using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;

namespace Membership.Features.Courses.GetEnrolledCourses;

public class GetEnrolledCoursesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/courses/enrolled", Handle)
            .WithName("GetEnrolledCourses")
            .WithTags("Courses")
            .WithDescription("Returns the list of courses the authenticated member is enrolled in.")
            .RequireAuthorization("MemberOnly")
            .Produces<List<GetEnrolledCoursesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonArray
                {
                    new JsonObject
                    {
                        ["id"] = Guid.NewGuid().ToString(),
                        ["name"] = "Yoga Basics",
                        ["coachName"] = "Coach Sara",
                        ["description"] = "Beginner yoga class focusing on flexibility."
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
        GetEnrolledCoursesHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(
            new GetEnrolledCoursesQuery(Guid.Empty), ct);

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
