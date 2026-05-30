using Activity.Shared;
using Carter;
using ErrorOr;

namespace Activity.Features.Members.GetMemberActivity;

public class GetMemberActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/activity/members/{memberId:guid}", Handle)
            .WithName("GetMemberActivity")
            .WithTags("Members")
            .WithDescription("Returns the activity history of a specific member: sessions attended, entry time, exit time.")
            .RequireAuthorization("AdminCoachOrMember")
            .Produces<List<GetMemberActivityResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(Guid memberId, GetMemberActivityHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(new GetMemberActivityQuery(memberId), ct);
        return result.Match(r => Results.Ok(r), errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
