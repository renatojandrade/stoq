using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Shopping.Domain.Aggregates;

namespace Stoq.Shopping.Infrastructure.Persistence.Configurations;

internal sealed class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
{
    public void Configure(EntityTypeBuilder<ShoppingListItem> builder)
    {
        builder.ToTable("ShoppingListItems");

        builder.HasKey(i => i.Id);

        builder.Ignore(i => i.DomainEvents);

        builder.Property(i => i.HouseholdId).IsRequired();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Quantity).IsRequired().HasPrecision(18, 4);
        builder.Property(i => i.Unit).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Notes).HasMaxLength(500);
        builder.Property(i => i.IsChecked).IsRequired();
        builder.Property(i => i.Source).IsRequired();

        builder.HasIndex(i => i.HouseholdId);
        builder.HasIndex(i => new { i.HouseholdId, i.IsChecked });

        // Prevents race conditions in StockLowIntegrationEventConsumer from creating
        // duplicate active items for the same product in the same household.
        builder.HasIndex(i => new { i.HouseholdId, i.ProductName })
            .HasFilter("\"IsChecked\" = false")
            .IsUnique()
            .HasDatabaseName("UX_ShoppingListItems_HouseholdId_ProductName_Active");
    }
}
