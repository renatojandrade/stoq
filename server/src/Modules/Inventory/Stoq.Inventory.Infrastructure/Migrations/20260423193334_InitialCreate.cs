using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductBrand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProductImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MinQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_HouseholdId",
                schema: "inventory",
                table: "InventoryItems",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_HouseholdId_ExpiryDate",
                schema: "inventory",
                table: "InventoryItems",
                columns: ["HouseholdId", "ExpiryDate"]);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_HouseholdId_ProductId",
                schema: "inventory",
                table: "InventoryItems",
                columns: ["HouseholdId", "ProductId"]);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_HouseholdId_ProductName",
                schema: "inventory",
                table: "InventoryItems",
                columns: ["HouseholdId", "ProductName"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryItems",
                schema: "inventory");
        }
    }
}
