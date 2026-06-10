using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Inventory.Domain.Aggregates;

namespace Stoq.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(i => i.Id);

        // DomainEvents are an in-memory-only concept — never persisted.
        builder.Ignore(i => i.DomainEvents);

        // IsStockLow is a computed property — never persisted.
        builder.Ignore(i => i.IsStockLow);

        builder.Property(i => i.HouseholdId).IsRequired();
        builder.Property(i => i.ProductId);
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductBrand).HasMaxLength(200);
        builder.Property(i => i.ProductImageUrl).HasMaxLength(2000);
        builder.Property(i => i.Quantity).IsRequired().HasPrecision(18, 4);
        builder.Property(i => i.MinQuantity).IsRequired().HasPrecision(18, 4);
        builder.Property(i => i.Unit).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Notes).HasMaxLength(500);

        builder.HasIndex(i => i.HouseholdId);
        builder.HasIndex(i => new { i.HouseholdId, i.ProductId });
        builder.HasIndex(i => new { i.HouseholdId, i.ExpiryDate });

        // GIN trigram index supports word-order-independent and fuzzy-match searches.
        // Replaces the former B-tree (HouseholdId, ProductName) index.
        builder.HasIndex(i => i.ProductName)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_InventoryItems_ProductName_Trgm");
    }
}
