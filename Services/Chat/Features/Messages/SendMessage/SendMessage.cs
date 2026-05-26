using Chat.Common;
using Chat.Features.Messages.SendMessage;
using Chat.Hubs;
using Chat.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.Features.Messages.SendMessage;

public record SendMessageRequest(string Content);

public static class SendMessageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{conversationId:guid}/messages", Handle)
           .RequireAuthorization()
           .WithName("SendMessage");
    }

    private static async Task<IResult> Handle(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        ChatDbContext db,
        UserAccessor accessor,
        IHubContext<ChatHub> hub)
    {
        var userId = accessor.UserId;

        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (!isParticipant)
            return Results.Forbid();

        var message = new Domain.Entities.Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = request.Content.Trim(),
            SentAt = DateTime.UtcNow
        };

        db.Messages.Add(message);

        var conversation = await db.Conversations.FindAsync(conversationId);
        if (conversation != null)
            conversation.LastMessageAt = message.SentAt;

        await db.SaveChangesAsync();

        var sender = await db.Users.FindAsync(userId);

        var response = new MessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender?.UserName,
            sender?.ProfilePhotoUrl,
            message.Content,
            message.SentAt,
            message.IsRead
        );

        // Broadcast to all connected clients in the conversation group
        await hub.Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", response);

        return Results.Created($"/api/conversations/{conversationId}/messages/{message.Id}", response);
    }
}