using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Code).IsRequired().HasMaxLength(30);
        builder.Property(u => u.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(u => u.NamePt).IsRequired().HasMaxLength(100);

        builder.HasIndex(u => u.Code).IsUnique();
    }
}
