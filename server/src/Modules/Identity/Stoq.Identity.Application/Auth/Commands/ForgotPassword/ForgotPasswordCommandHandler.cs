using MediatR;
using Stoq.Identity.Application.Ports;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailService emailService)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Look up user silently â€” if not found, still return success to prevent enumeration.
        UserInfo? user = await identityService.FindByEmailAsync(request.Email, cancellationToken);

        if (user is not null)
        {
            var otp = await identityService.GeneratePasswordResetOtpAsync(user.Id, cancellationToken);
            await emailService.SendPasswordResetOtpAsync(user.Email, otp, cancellationToken);
        }

        return Result.Success();
    }
}
