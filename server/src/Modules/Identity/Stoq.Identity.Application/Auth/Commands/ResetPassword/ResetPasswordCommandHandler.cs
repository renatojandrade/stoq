using MediatR;
using Stoq.Identity.Application.Ports;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.ResetPassword;

internal sealed class ResetPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        UserInfo? user = await identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure("Invalid OTP or email.");
        }

        var success = await identityService.ResetPasswordAsync(
            user.Id, request.OtpCode, request.NewPassword, cancellationToken);

        if (success)
        {
            return Result.Success();
        }

        return Result.Failure("Invalid or expired OTP.");
    }
}
