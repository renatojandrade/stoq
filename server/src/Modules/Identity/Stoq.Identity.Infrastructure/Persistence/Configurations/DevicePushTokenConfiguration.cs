using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Identity.Domain.Entities;
using Stoq.Identity.Infrastructure.Persistence;

namespace Stoq.Identity.Infrastructure.Persistence.Configurations;

internal sealed class DevicePushTokenConfiguration : IEntityTypeConfiguration<DevicePushToken>
{
    public void Configure(EntityTypeBuilder<DevicePushToken> builder)
    {
        builder.ToTable("DevicePushTokens", IdentityModuleDbContext.Schema);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Token).IsRequired().HasMaxLength(200);
        builder.Property(t => t.CreatedAt).IsRequired();

        // One row per unique Expo token (tokens are device-scoped).
        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
