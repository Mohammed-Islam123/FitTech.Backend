using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Notification.Api.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            config["Email:SenderName"],
            config["Email:SenderEmail"]));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            config["Email:Host"],
            int.Parse(config["Email:Port"]!),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            config["Email:UserName"],
            config["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}