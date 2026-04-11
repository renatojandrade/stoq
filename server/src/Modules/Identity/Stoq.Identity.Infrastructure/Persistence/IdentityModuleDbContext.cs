using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Infrastructure.Entities;

namespace Stoq.Identity.Infrastructure.Persistence;

public sealed class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>>(options)
{
    public const string Schema = "identity";

    public DbSet<Household> Households => Set<Household>();

    public DbSet<HouseholdMember> HouseholdMembers => Set<HouseholdMember>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Move all ASP.NET Identity tables into the "identity" schema.
        foreach (IMutableEntityType? entity in builder.Model.GetEntityTypes().Where(e => e.GetTableName() is not null))
        {
            entity.SetSchema(Schema);
        }

        // Apply all IEntityTypeConfiguration classes from this assembly.
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityModuleDbContext).Assembly);

        // MassTransit Outbox tables.
        builder.AddInboxStateEntity(e => e.ToTable("InboxState", Schema));
        builder.AddOutboxStateEntity(e => e.ToTable("OutboxState", Schema));
        builder.AddOutboxMessageEntity(e => e.ToTable("OutboxMessage", Schema));
    }
}
