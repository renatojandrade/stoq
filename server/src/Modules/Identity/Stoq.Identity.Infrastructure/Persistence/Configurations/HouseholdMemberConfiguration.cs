using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Domain.Enums;

namespace Stoq.Identity.Infrastructure.Persistence.Configurations;

internal sealed class HouseholdMemberConfiguration : IEntityTypeConfiguration<HouseholdMember>
{
    public void Configure(EntityTypeBuilder<HouseholdMember> builder)
    {
        builder.HasKey(m => new { m.HouseholdId, m.UserId });

        builder.Property(m => m.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_HouseholdMembers_Role",
            "\"Role\" IN ('HouseholdAdmin', 'HouseholdMember', 'HouseholdViewer')"));

        builder.Property(m => m.JoinedAt).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).IsRequired();
    }
}
