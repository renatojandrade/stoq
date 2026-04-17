using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryAndNullableHouseholdId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make HouseholdId nullable (reference products have no household owner).
            migrationBuilder.AlterColumn<Guid>(
                name: "HouseholdId",
                schema: "catalog",
                table: "Products",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Country of origin/relevance for the product. NULL = available to all countries.
            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "catalog",
                table: "Products",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            // Unique index on Barcode (partial — only non-null barcodes).
            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                schema: "catalog",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "\"Barcode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Country",
                schema: "catalog",
                table: "Products",
                column: "Country");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Country",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "catalog",
                table: "Products");

            migrationBuilder.AlterColumn<Guid>(
                name: "HouseholdId",
                schema: "catalog",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
