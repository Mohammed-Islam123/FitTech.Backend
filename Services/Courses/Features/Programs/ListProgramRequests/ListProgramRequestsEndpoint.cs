using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Courses.Features.Programs.ListProgramRequests;

public class ListProgramRequestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/requests", Handle)
            .WithName("ListProgramRequests")
            .WithTags("Programs")
            .WithDescription("Returns all program creation requests pending admin review.")
            .RequireAuthorization("AdminOnly")
            .Produces<List<ListProgramRequestsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<List<ListProgramRequestsResponse>>>(
            new ListProgramRequestsQuery(), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
