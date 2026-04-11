using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using Stoq.Identity.Application.Ports;

namespace Stoq.Identity.Infrastructure.Services;

internal sealed class EmailServiceImpl(
    IResend resend,
    IConfiguration configuration,
    ILogger<EmailServiceImpl> logger)
    : IEmailService
{
    public async Task SendPasswordResetOtpAsync(string toEmail, string otpCode, CancellationToken ct = default)
    {
        await SendAsync(
            to: toEmail,
            subject: "Your Stoq password reset code",
            html: $"<p>Your password reset code is:</p><h2>{otpCode}</h2><p>This code expires in 15 minutes.</p>",
            ct: ct);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string displayName, CancellationToken ct = default)
    {
        await SendAsync(
            to: toEmail,
            subject: "Welcome to Stoq!",
            html: $"<p>Hi <strong>{displayName}</strong>,</p><p>Welcome to Stoq! Your account is ready.</p>",
            ct: ct);
    }

    private async Task SendAsync(string to, string subject, string html, CancellationToken ct)
    {
        var apiKey = configuration["Resend:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("Resend:ApiKey is not configured — email not sent to {To}.", to);
            return;
        }

        var fromEmail = configuration["Resend:FromEmail"]
            ?? throw new InvalidOperationException("Resend:FromEmail is not configured.");
        var fromName = configuration["Resend:FromName"] ?? "Stoq";

        EmailMessage message = new()
        {
            From = $"{fromName} <{fromEmail}>",
            To = { to },
            Subject = subject,
            HtmlBody = html,
        };

        ResendResponse<Guid> response = await resend.EmailSendAsync(message, ct);

        logger.LogInformation("Email sent via Resend. Id: {Id}", response.Content);
    }
}
