using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Courses.Features.Coaches.GetCoachClients;

public class GetCoachClientsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coaches/{coachId:guid}/clients", Handle)
            .WithName("GetCoachClients")
            .WithTags("Coaches")
            .WithDescription("Returns all members subscribed to any of the coach's programs.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetCoachClientsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid coachId,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<List<GetCoachClientsResponse>>>(
            new GetCoachClientsQuery(coachId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
