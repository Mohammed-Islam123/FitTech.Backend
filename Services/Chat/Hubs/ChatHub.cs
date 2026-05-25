using Chat.Common;
using Chat.Features.Messages.SendMessage;
using Chat.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.Hubs;

[Authorize]
public class ChatHub(ChatDbContext db, UserAccessor accessor) : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        var userId = accessor.UserId;
        var convId = Guid.Parse(conversationId);

        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == convId && cp.UserId == userId);

        if (!isParticipant)
        {
            throw new HubException("You are not a participant of this conversation.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public Task LeaveConversation(string conversationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

    public async Task SendMessage(string conversationId, string content)
    {
        var userId = accessor.UserId;
        var convId = Guid.Parse(conversationId);

        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == convId && cp.UserId == userId);

        if (!isParticipant)
            throw new HubException("You are not a participant of this conversation.");

        var message = new Domain.Entities.Message
        {
            ConversationId = convId,
            SenderId = userId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };

        db.Messages.Add(message);

        var conversation = await db.Conversations.FindAsync(convId);
        if (conversation != null)
            conversation.LastMessageAt = message.SentAt;

        await db.SaveChangesAsync();

        var sender = await db.Users.FindAsync(userId);

        var response = new MessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender?.UserName ?? sender?.Email ?? "Unknown",
            sender?.ProfilePhotoUrl,
            message.Content,
            message.SentAt,
            message.IsRead
        );

        await Clients.Group(conversationId).SendAsync("ReceiveMessage", response);
    }

    public async Task MarkAsRead(string conversationId)
    {
        var userId = accessor.UserId;
        var convId = Guid.Parse(conversationId);

        var unread = await db.Messages
            .Where(m => m.ConversationId == convId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var msg in unread)
            msg.IsRead = true;

        await db.SaveChangesAsync();

        await Clients.Group(conversationId).SendAsync("MessagesRead", userId);
    }
}