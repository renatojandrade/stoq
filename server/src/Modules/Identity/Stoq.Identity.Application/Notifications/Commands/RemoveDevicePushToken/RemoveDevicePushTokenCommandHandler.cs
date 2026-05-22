using MediatR;
using Stoq.Identity.Application.Ports;

namespace Stoq.Identity.Application.Notifications.Commands.RemoveDevicePushToken;

internal sealed class RemoveDevicePushTokenCommandHandler(IDevicePushTokenRepository repository)
    : IRequestHandler<RemoveDevicePushTokenCommand>
{
    public async Task Handle(RemoveDevicePushTokenCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveByUserAsync(request.UserId, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
