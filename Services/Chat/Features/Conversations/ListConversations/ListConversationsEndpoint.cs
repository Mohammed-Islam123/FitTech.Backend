using Chat.Common;
using Chat.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Chat.Features.Conversations.ListConversations;

public static class ListConversationsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations", Handle)
           .RequireAuthorization()
           .WithName("ListConversations");
    }

    private static async Task<IResult> Handle(
        ChatDbContext db,
        UserAccessor accessor)
    {
        var userId = accessor.UserId;

        var conversations = await db.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();

        var result = conversations.Select(c =>
        {
            var lastMessage = c.Messages.FirstOrDefault();
            var other = c.Participants.FirstOrDefault(p => p.UserId != userId)?.User;

            return new
            {
                c.Id,
                c.CreatedAt,
                c.LastMessageAt,
                OtherUser = other is null ? null : new
                {
                    other.Id,
                    other.UserName,
                    other.ProfilePhotoUrl,
                    other.IsCoach
                },
                LastMessage = lastMessage is null ? null : new
                {
                    lastMessage.Content,
                    lastMessage.SentAt,
                    lastMessage.IsRead
                }
            };
        });

        return Results.Ok(result);
    }
}