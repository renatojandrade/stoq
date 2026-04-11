using MediatR;
using Stoq.Identity.Application.Ports;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.Logout;

internal sealed class LogoutCommandHandler(IIdentityService identityService)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        return Result.Success();
    }
}
