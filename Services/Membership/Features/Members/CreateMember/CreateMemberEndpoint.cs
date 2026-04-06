using Carter;
using ErrorOr;
using Wolverine;

namespace Membership.Features.Members.CreateMember;

public class CreateMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/members", Handle)
            .WithName("CreateMember")
            .RequireAuthorization("AdminOnly")
            .Produces<CreateMemberResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    /// <description>
    /// Creates a new member, provisions an identity account, activates their subscription to a plan, and assigns an NFC card if provided.
    /// </description>
    /// <example>
    /// POST /api/members
    /// Body: 
    /// { 
    ///   "firstName": "John", 
    ///   "lastName": "Doe", 
    ///   "email": "john.doe@example.com",
    ///   "phoneNumber": "1234567890",
    ///   "dateOfBirth": "1990-01-01",
    ///   "gender": 1,
    ///   "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "cardUid": "AABBCCDD"
    /// }
    /// Response: { "memberId": "1fa85f64-5717-4562-b3fc-2c963f66afa6" }
    /// </example>
    private static async Task<IResult> Handle(
        CreateMemberRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<CreateMemberResponse>>(new CreateMemberCommand(request), ct);

        return result.Match(
            response => Results.Created($"/api/members/{response.MemberId}", response),
            errors => MapErrorsToResult(errors));
    }

    private static IResult MapErrorsToResult(List<Error> errors)
    {
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            statusCode: statusCode,
            title: firstError.Description,
            detail: firstError.Code);
    }
}
