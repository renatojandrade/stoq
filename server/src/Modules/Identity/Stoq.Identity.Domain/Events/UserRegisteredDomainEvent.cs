using Stoq.Shared.Kernel.Domain;

namespace Stoq.Identity.Domain.Events;
public sealed class UserRegisteredDomainEvent(Guid userId, string email, string displayName) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
    public string DisplayName { get; } = displayName;
}
