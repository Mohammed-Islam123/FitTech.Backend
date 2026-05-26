// using MassTransit;
// using Notification.Api.Infrastructure;
// using Notification.Api.Services;
// using Shared.Events;

// namespace Notification.Api.Consumers;

// public class UserRegisteredConsumer(
//     IIdentityClient identityClient,
//     IEmailService emailService,
//     IConfiguration config) : IConsumer<UserCreatedEvent>
// {
//     public async Task Consume(ConsumeContext<UserCreatedEvent> context)
//     {
//         var @event = context.Message;

//         var result = await identityClient.GetConfirmationTokenAsync(@event.Email!);
//         if (result is null) return;

//         var (userId, token) = result.Value;
//         var encodedToken = Uri.EscapeDataString(token);
//         var frontendUrl = config["Frontend:BaseUrl"];

//         // This link opens your frontend, which then calls Identity's verify-email
//         var confirmUrl = $"{frontendUrl}/confirm-email?userId={userId}&token={encodedToken}";
//         var html = $"""
//             <h2>Welcome to FitTech, {@event.FirstName}!</h2>
//             <p>Please confirm your email address by clicking the button below:</p>
//             <a href="{confirmUrl}" 
//                style="background:#4CAF50;color:white;padding:12px 24px;
//                       text-decoration:none;border-radius:4px;display:inline-block">
//                Confirm Email
//             </a>
//             <p>If you did not register, ignore this email.</p>
//             """;

//         await emailService.SendEmailAsync(
//             @event.Email!,
//             $"{@event.FirstName} {@event.LastName}",
//             "Confirm your FitTech email",
//             html);
//     }
// }