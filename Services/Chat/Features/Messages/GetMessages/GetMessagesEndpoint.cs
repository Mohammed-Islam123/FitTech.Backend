using Chat.Common;
using Chat.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.Features.Messages.GetMessages;

public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string? SenderName,
    string? SenderPhotoUrl,
    string Content,
    DateTime SentAt,
    bool IsRead
);

public static class GetMessagesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations/{conversationId:guid}/messages", Handle)
           .RequireAuthorization()
           .WithName("GetMessages");
    }

    private static async Task<IResult> Handle(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        ChatDbContext db = default!,
        UserAccessor accessor = default!)
    {
        var userId = accessor.UserId;

        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (!isParticipant)
            return Results.Forbid();

        var total = await db.Messages.CountAsync(m => m.ConversationId == conversationId);

        var messages = await db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .Select(m => new MessageResponse(
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.Sender.UserName,
                m.Sender.ProfilePhotoUrl,
                m.Content,
                m.SentAt,
                m.IsRead
            ))
            .ToListAsync();

        return Results.Ok(new
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Messages = messages.AsEnumerable().Reverse()
        });
    }
}