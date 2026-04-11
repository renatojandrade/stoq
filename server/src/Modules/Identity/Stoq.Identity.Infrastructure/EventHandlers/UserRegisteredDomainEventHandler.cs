using MediatR;
using Microsoft.Extensions.Logging;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Events;

namespace Stoq.Identity.Infrastructure.EventHandlers;

internal sealed class UserRegisteredDomainEventHandler(
    IEmailService emailService,
    ILogger<UserRegisteredDomainEventHandler> logger)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await emailService.SendWelcomeEmailAsync(notification.Email, notification.DisplayName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Welcome email could not be sent to {Email}.", notification.Email);
        }
    }
}
