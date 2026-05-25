using Chat.Common;
using Chat.Domain.Entities;
using Chat.Features.Conversations.GetOrCreate.Dtos;
using Chat.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.Features.Conversations.GetOrCreate;

public record GetOrCreateConversationRequest(Guid OtherUserId);


public static class GetOrCreateConversationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations", Handle)
           .RequireAuthorization()
           .WithName("GetOrCreateConversation");
    }

    private static async Task<IResult> Handle(
        [FromBody] GetOrCreateConversationRequest request,
        ChatDbContext db,
        UserAccessor accessor)
    {
        var currentUserId = accessor.UserId;
        var otherUserId = request.OtherUserId;

        if (currentUserId == otherUserId)
            return Results.BadRequest("Cannot start a conversation with yourself.");

        var currentUser = await db.Users.FindAsync(currentUserId);
        var otherUser = await db.Users.FindAsync(otherUserId);

        if (currentUser is null || otherUser is null)
            return Results.NotFound("One or both users were not found.");

        if (!currentUser.IsCoach && !otherUser.IsCoach)
            return Results.BadRequest("A conversation must involve at least one coach.");

        if (currentUser.IsCoach && otherUser.IsCoach)
            return Results.BadRequest("A conversation cannot be between two coaches.");

        var existing = await db.Conversations
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Where(c =>
                c.Participants.Any(p => p.UserId == currentUserId) &&
                c.Participants.Any(p => p.UserId == otherUserId) &&
                c.Participants.Count == 2)
            .FirstOrDefaultAsync();

        if (existing is not null)
            return Results.Ok(MapToResponse(existing));

        var conversation = new Conversation();
        conversation.Participants.Add(new ConversationParticipant { UserId = currentUserId });
        conversation.Participants.Add(new ConversationParticipant { UserId = otherUserId });

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync();

        var created = await db.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .FirstAsync(c => c.Id == conversation.Id);

        return Results.Created($"/api/conversations/{created.Id}", MapToResponse(created));
    }

    private static ConversationResponse MapToResponse(Conversation c) => new(
        c.Id,
        c.CreatedAt,
        c.LastMessageAt,
        c.Participants.Select(p => new ParticipantDto(
            p.UserId,
            p.User?.UserName,
            p.User?.ProfilePhotoUrl,
            p.User?.IsCoach ?? false
        )).ToList()
    );
}