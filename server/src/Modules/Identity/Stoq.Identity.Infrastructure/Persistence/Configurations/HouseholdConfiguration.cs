using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Identity.Domain.Aggregates;
using Stoq.Identity.Domain.ValueObjects;

namespace Stoq.Identity.Infrastructure.Persistence.Configurations;

internal sealed class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.HasKey(h => h.Id);

        // DomainEvents are an in-memory-only concept â€” never persisted.
        builder.Ignore(h => h.DomainEvents);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.Country)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        // InviteCode owned value object â€” stored as two columns.
        builder.OwnsOne(h => h.InviteCode, invite =>
        {
            invite.Property(ic => ic.Value)
                .HasColumnName("InviteCodeValue")
                .IsRequired()
                .HasMaxLength(InviteCode.Length);

            invite.Property(ic => ic.ExpiresAt)
                .HasColumnName("InviteCodeExpiresAt")
                .IsRequired();

            invite.HasIndex(ic => ic.Value)
                .IsUnique();
        });

        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.CreatedBy).IsRequired();

        builder.HasMany(h => h.Members)
            .WithOne()
            .HasForeignKey(m => m.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // Suppress EF Core navigation back-reference warnings.
        builder.Metadata.FindNavigation(nameof(Household.Members))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
