using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToParseCorrectionItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "inventory",
                table: "ParseCorrectionItems");
        }
    }
}
