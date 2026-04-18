using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Membership.Features.Plans.CreatePlan;

public class CreatePlanEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plans", Handle)
            .WithName("CreatePlan")
            .WithTags("Plans")
            .WithDescription("Creates a new subscription plan. Restricted to Administrators.")
            .Produces<CreatePlanResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        CreatePlanRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<CreatePlanResponse>>(new CreatePlanCommand(request), ct);
        return result.Match(
            response => Results.Created($"/api/plans/{response.PlanId}", response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
