using MediatR;

namespace Stoq.Identity.Application.Notifications.Commands.RemoveDevicePushToken;

public sealed record RemoveDevicePushTokenCommand(Guid UserId) : IRequest;
