using GiapTech.Ipages.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GiapTech.Ipages.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => SendAsync([to], subject, htmlBody, ct);

    public async Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _config["SmtpSettings:Host"];
        if (string.IsNullOrEmpty(host))
        {
            _logger.LogWarning("SMTP host not configured, skipping email: {Subject}", subject);
            return;
        }

        var port = int.Parse(_config["SmtpSettings:Port"] ?? "587");
        var username = _config["SmtpSettings:Username"] ?? string.Empty;
        var password = _config["SmtpSettings:Password"] ?? string.Empty;
        var fromEmail = _config["SmtpSettings:FromEmail"] ?? "noreply@giaptech.io";
        var fromName = _config["SmtpSettings:FromName"] ?? "GiapTech.Ipages";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        foreach (var recipient in to)
            message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
        if (!string.IsNullOrEmpty(username))
            await client.AuthenticateAsync(username, password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email '{Subject}' sent to {Recipients}", subject, string.Join(", ", to));
    }
}
