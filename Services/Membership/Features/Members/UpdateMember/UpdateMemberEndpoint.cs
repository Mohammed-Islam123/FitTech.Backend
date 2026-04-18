using System.Text.Json.Nodes;
using Carter;
using ErrorOr;
using Membership.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Membership.Features.Members.UpdateMember;

public class UpdateMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members");

        group.MapPut("/{id:guid}", UpdateById)
            .WithName("UpdateMemberById")
            .DisableAntiforgery() // Using multipart form
            .WithTags("Members")
            .WithDescription("Updates a specific member's details. Restricted to Admins and Coaches.")
            .Produces<UpdateMemberResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleData = new JsonObject
                {
                    ["firstName"] = "John",
                    ["lastName"] = "Doe",
                    ["phoneNumber"] = "1234567890",
                    ["gender"] = 1,
                    ["dateOfBirth"] = "1990-01-01",
                    ["objectives"] = "Lose weight",
                    ["medicalRestrictions"] = "None",
                    ["status"] = 1
                };

                if (operation.RequestBody?.Content?.TryGetValue("multipart/form-data", out var reqContent) == true)
                {
                    reqContent.Example = exampleData;
                }
                return Task.CompletedTask;
            });

        group.MapPut("/my-profile", UpdateSelf)
            .WithName("UpdateMyProfile")
            .DisableAntiforgery()
            .WithTags("Members")
            .WithDescription("Updates the current authenticated member's profile.")
            .Produces<UpdateMemberResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                var exampleData = new JsonObject
                {
                    ["firstName"] = "Jane",
                    ["lastName"] = "Smith",
                    ["phoneNumber"] = "0987654321",
                    ["gender"] = 2,
                    ["dateOfBirth"] = "1995-05-05",
                    ["objectives"] = "Gain muscle",
                    ["medicalRestrictions"] = "Back pain",
                    ["oldPassword"] = "OldP@ss123",
                    ["newPassword"] = "NewP@ss123"
                };

                if (operation.RequestBody?.Content?.TryGetValue("multipart/form-data", out var reqContent) == true)
                {
                    reqContent.Example = exampleData;
                }
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> UpdateById(
        Guid id,
        [FromForm] UpdateMemberRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<UpdateMemberResponse>>(new UpdateMemberCommand(request, id), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }

    private static async Task<IResult> UpdateSelf(
        [FromForm] UpdateMemberRequest request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<UpdateMemberResponse>>(new UpdateMemberCommand(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }


}
