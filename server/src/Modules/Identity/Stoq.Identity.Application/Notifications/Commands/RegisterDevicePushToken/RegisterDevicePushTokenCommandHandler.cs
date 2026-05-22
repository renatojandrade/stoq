using MediatR;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Entities;

namespace Stoq.Identity.Application.Notifications.Commands.RegisterDevicePushToken;

internal sealed class RegisterDevicePushTokenCommandHandler(IDevicePushTokenRepository repository)
    : IRequestHandler<RegisterDevicePushTokenCommand>
{
    public async Task Handle(RegisterDevicePushTokenCommand request, CancellationToken cancellationToken)
    {
        var token = DevicePushToken.Create(request.UserId, request.Token);
        await repository.UpsertAsync(token, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
