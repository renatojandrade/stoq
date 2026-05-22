using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.HouseholdId).IsRequired(false);

        // Two partial unique indexes to correctly handle the NULL household case in PostgreSQL.
        builder.HasIndex(b => b.Name)
            .IsUnique()
            .HasFilter("\"HouseholdId\" IS NULL")
            .HasDatabaseName("IX_Brands_Name_Global");

        builder.HasIndex(b => new { b.Name, b.HouseholdId })
            .IsUnique()
            .HasFilter("\"HouseholdId\" IS NOT NULL")
            .HasDatabaseName("IX_Brands_Name_Household");

        builder.HasIndex(b => b.HouseholdId);
    }
}
