namespace Stoq.Shared.Contracts.Entities;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
    Guid UpdatedBy { get; set; }
}
