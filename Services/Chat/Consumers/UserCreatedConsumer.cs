using Chat.Domain.Entities;
using Chat.Infrastructure;
using MassTransit;
using Shared.Events;

namespace Chat.Consumers;

public class UserCreatedConsumer(ChatDbContext db) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;

        var exists = await db.Users.FindAsync(@event.UserId);
        if (exists is not null) return;

        db.Users.Add(new User
        {
            Id = @event.UserId,
            UserName = @event.UserName,
            Email = @event.Email,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            IsActive = true,
            IsCoach = @event.IsCoach
        });

        await db.SaveChangesAsync();
    }
}