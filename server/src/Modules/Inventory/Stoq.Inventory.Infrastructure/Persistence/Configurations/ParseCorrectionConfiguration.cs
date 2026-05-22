using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stoq.Inventory.Domain.Entities;
using Stoq.Inventory.Domain.Enums;

namespace Stoq.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ParseCorrectionConfiguration : IEntityTypeConfiguration<ParseCorrection>
{
    public void Configure(EntityTypeBuilder<ParseCorrection> builder)
    {
        builder.ToTable("ParseCorrections");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.HouseholdId).IsRequired();
        builder.Property(p => p.QrCodeUrl).IsRequired().HasMaxLength(2048);
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ParseCorrectionStatus.Pending);
        builder.Property(p => p.RetailerName).HasMaxLength(200);
        builder.Property(p => p.RetailerCnpj).HasMaxLength(14);
        builder.Property(p => p.RetailerTradeName).HasMaxLength(200);
        builder.Property(p => p.RetailerStreet).HasMaxLength(300);
        builder.Property(p => p.RetailerCity).HasMaxLength(100);
        builder.Property(p => p.RetailerState).HasMaxLength(2);
        builder.Property(p => p.RetailerZipCode).HasMaxLength(8);
        builder.Property(p => p.ReceiptNumber).HasMaxLength(20);
        builder.Property(p => p.ParsedItemCount).IsRequired();
        builder.Property(p => p.FailureReason).HasMaxLength(1000);
        builder.Property(p => p.InitiatedByUserId).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.ParsedAt);

        builder.HasIndex(p => p.HouseholdId);
        builder.HasIndex(p => new { p.HouseholdId, p.RetailerName });
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => new { p.InitiatedByUserId, p.Status });

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.ParseCorrectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ParseCorrectionItemConfiguration : IEntityTypeConfiguration<ParseCorrectionItem>
{
    public void Configure(EntityTypeBuilder<ParseCorrectionItem> builder)
    {
        builder.ToTable("ParseCorrectionItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ParseCorrectionId).IsRequired();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Quantity).IsRequired().HasPrecision(18, 4);
        builder.Property(i => i.Unit).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Barcode).HasMaxLength(14);
        builder.Property(i => i.RetailerSku).HasMaxLength(60);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 4);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 4);
        builder.Property(i => i.Discount).HasPrecision(18, 4);
        builder.Property(i => i.Currency).HasMaxLength(3);
        builder.Property(i => i.MatchStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(MatchStatus.Pending);

        builder.HasIndex(i => i.ParseCorrectionId);
        builder.HasIndex(i => i.Barcode);
        builder.HasIndex(i => i.MatchStatus);
    }
}
