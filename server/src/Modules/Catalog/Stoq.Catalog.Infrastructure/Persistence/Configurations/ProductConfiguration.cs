using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Catalog.Domain.Aggregates;

namespace Stoq.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // DomainEvents are an in-memory-only concept — never persisted.
        builder.Ignore(p => p.DomainEvents);

        // NULL = reference product (no household owner).
        builder.Property(p => p.HouseholdId).IsRequired(false);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Brand).HasMaxLength(200);
        builder.Property(p => p.Category).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Unit).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.MinQuantity).IsRequired().HasPrecision(18, 4);
        builder.Property(p => p.ImageUrl).HasMaxLength(2000);
        builder.Property(p => p.Barcode).HasMaxLength(14);
        builder.Property(p => p.Country).HasMaxLength(10);

        builder.HasIndex(p => p.HouseholdId);
        builder.HasIndex(p => new { p.HouseholdId, p.Name });
        builder.HasIndex(p => p.Barcode).IsUnique().HasFilter("\"Barcode\" IS NOT NULL");
        builder.HasIndex(p => p.Country);
    }
}
