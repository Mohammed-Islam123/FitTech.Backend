using MassTransit;
using Shared.Events;
using System;
namespace Notification.Api.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;
        Console.WriteLine($"New member registered — sending welcome email to {evt.Email} (UserId: {evt.UserId}, Name: {evt.FullName})");
        _logger.LogInformation(
            "New member registered — sending welcome email to {Email} (UserId: {UserId}, Name: {FullName})",
            evt.Email, evt.UserId, evt.FullName);


        // NOTE : here is te andling : like sending an emai via mailgun or smtp 

        return Task.CompletedTask;
    }
}