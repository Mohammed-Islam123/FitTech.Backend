using Carter;
using Courses.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Courses.Features.Programs.GetProgramMembers;

public class GetProgramMembersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs/{programId:guid}/members", Handle)
            .WithName("GetProgramMembers")
            .WithTags("Programs")
            .WithDescription("Returns the list of members enrolled in a specific course for attendance tracking.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetProgramMembersResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        Guid programId,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<List<GetProgramMembersResponse>>>(
            new GetProgramMembersQuery(programId), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
