using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Resturant_Backend.Helpers;

namespace Resturant_Backend.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string mailTo, string subject, string body)
    {
        var email = new MimeMessage();

        // 1. setting the sender's email and display name  
        email.Sender = MailboxAddress.Parse(_emailSettings.Email);
        email.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email));

        // 2. adding the recipient's email and subject
        email.To.Add(MailboxAddress.Parse(mailTo));
        email.Subject = subject;

        // 3. building the email body with HTML content
        var builder = new BodyBuilder
        {
            HtmlBody = body
        };
        email.Body = builder.ToMessageBody();

        // 4. connecting to the SMTP server, authenticating, sending the email, and disconnecting
        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}