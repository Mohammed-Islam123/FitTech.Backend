using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Membership.Features.Members.GetMyProfile;

public class GetMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me", Handle)
            .WithName("GetMyProfile")
            .WithTags("Members")
            .WithDescription("Returns the authenticated member's personal details including goals and medical file ID.")
            .RequireAuthorization("MemberOnly")
            .Produces<GetMyProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleResponse = new JsonObject
                {
                    ["fullName"] = "John Doe",
                    ["gender"] = null,
                    ["dateOfBirth"] = null,
                    ["phoneNumber"] = "+213-555-123456",
                    ["email"] = "john.doe@example.com",
                    ["emailConfirmed"] = true,
                    ["accountCreationDate"] = DateTime.UtcNow.AddYears(-1).ToString("O"),
                    ["membershipDurationYears"] = 1,
                    ["isActive"] = true,
                    ["profilePictureUrl"] = "http://identity-api/profile-photos/abc.jpg",
                    ["goals"] = "Lose weight, build muscle",
                    ["medicalFileId"] = Guid.NewGuid().ToString()
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
        var result = await messageBus.InvokeAsync<ErrorOr<GetMyProfileResponse>>(
            new GetMyProfileQuery(), ct);

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
