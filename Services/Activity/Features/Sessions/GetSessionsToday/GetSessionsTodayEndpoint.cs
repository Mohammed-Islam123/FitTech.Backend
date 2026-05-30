using Activity.Shared;
using Carter;
using ErrorOr;

namespace Activity.Features.Sessions.GetSessionsToday;

public class GetSessionsTodayEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/activity/sessions/today", Handle)
            .WithName("GetSessionsToday")
            .WithTags("Sessions")
            .WithDescription("Returns all member sessions (check-ins) occurring today.")
            .RequireAuthorization("AdminOrCoach")
            .Produces<List<GetSessionsTodayResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(GetSessionsTodayHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(new GetSessionsTodayQuery(), ct);
        return result.Match(r => Results.Ok(r), errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
