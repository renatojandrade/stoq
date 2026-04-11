namespace Stoq.Identity.Application.Ports;
public interface IEmailService
{
    Task SendPasswordResetOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string toEmail, string displayName, CancellationToken ct = default);
}
