using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Catalog.Domain.Entities;

namespace Stoq.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(c => c.NamePt).IsRequired().HasMaxLength(100);
    }
}
