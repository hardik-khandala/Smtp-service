using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PortfolioBackend.Configuration;
using PortfolioBackend.Contacts;

namespace PortfolioBackend.Services
{
    public class ZohoEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<ZohoEmailService> _logger;

        public ZohoEmailService(IOptions<SmtpSettings> options, ILogger<ZohoEmailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }
        public async Task SendContactEmailAsync(ContactRequest request, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();

            // 1. Zoho strictly checks that the 'From' address belongs to the authenticated Zoho account
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress("Portfolio Admin", _settings.RecipientEmail));

            // 2. Setting Reply-To allows you to simply hit "Reply" in your email client to respond to the visitor
            message.ReplyTo.Add(new MailboxAddress(request.Name, request.Email));

            message.Subject = $"[Portfolio Contact] {request.Subject.Trim()}";

            // 3. Build HTML & Plaintext bodies with HTML-encoding to prevent injection attacks
            var encodedName = WebUtility.HtmlEncode(request.Name);
            var encodedEmail = WebUtility.HtmlEncode(request.Email);
            var encodedSubject = WebUtility.HtmlEncode(request.Subject);
            var encodedMessage = WebUtility.HtmlEncode(request.Message).Replace("\n", "<br/>");

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"""
                <div style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                    <h2 style="color: #0056b3; border-bottom: 2px solid #eaeaea; padding-bottom: 8px;">
                        New Contact Inquiry
                    </h2>
                    <table style="width: 100%; border-collapse: collapse; margin-bottom: 20px;">
                        <tr>
                            <td style="width: 120px; font-weight: bold; padding: 6px 0;">From:</td>
                            <td>{encodedName}</td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; padding: 6px 0;">Email:</td>
                            <td><a href="mailto:{encodedEmail}">{encodedEmail}</a></td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; padding: 6px 0;">Subject:</td>
                            <td>{encodedSubject}</td>
                        </tr>
                    </table>
                    <div style="background-color: #f8f9fa; border-left: 4px solid #0056b3; padding: 15px; border-radius: 4px;">
                        <h4 style="margin-top: 0; color: #555;">Message Content:</h4>
                        <p style="white-space: pre-wrap; margin-bottom: 0;">{encodedMessage}</p>
                    </div>
                    <p style="font-size: 12px; color: #888; margin-top: 25px;">
                        Sent automatically from your portfolio website contact endpoint.
                    </p>
                </div>
            """,
                TextBody = $"""
                New Contact Form Submission:
                ----------------------------
                From: {request.Name}
                Email: {request.Email}
                Subject: {request.Subject}

                Message:
                {request.Message}
            """
            };

            message.Body = bodyBuilder.ToMessageBody();

            // 4. Connect, Authenticate, and Transmit via MailKit
            using var client = new SmtpClient();

            try
            {
                // Port 587 uses StartTls; Port 465 uses SslOnConnect
                var socketOption = _settings.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                _logger.LogInformation("Connecting to Zoho SMTP host {Host}:{Port}...", _settings.Host, _settings.Port);
                await client.ConnectAsync(_settings.Host, _settings.Port, socketOption, cancellationToken);

                _logger.LogInformation("Authenticating with Zoho Mail as {Username}...", _settings.Username);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

                _logger.LogInformation("Sending email from {Sender} to {Recipient}...", _settings.SenderEmail, _settings.RecipientEmail);
                await client.SendAsync(message, cancellationToken);

                await client.DisconnectAsync(true, cancellationToken);
                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver contact email via Zoho SMTP.");
                throw; // Rethrow to let the controller/middleware handle the 500 response
            }
        }
    }
}
