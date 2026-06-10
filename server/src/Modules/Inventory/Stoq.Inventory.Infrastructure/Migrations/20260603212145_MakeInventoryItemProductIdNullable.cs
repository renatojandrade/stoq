using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeInventoryItemProductIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                schema: "inventory",
                table: "InventoryItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                schema: "inventory",
                table: "InventoryItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldNullable: true,
                oldType: "uuid");
        }
    }
}
