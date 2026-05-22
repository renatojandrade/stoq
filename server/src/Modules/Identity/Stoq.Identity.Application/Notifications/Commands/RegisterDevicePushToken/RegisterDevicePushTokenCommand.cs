using MediatR;

namespace Stoq.Identity.Application.Notifications.Commands.RegisterDevicePushToken;

public sealed record RegisterDevicePushTokenCommand(Guid UserId, string Token) : IRequest;
