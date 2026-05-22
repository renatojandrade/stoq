using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParseCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS inventory.\"IX_InventoryItems_HouseholdId_ProductName\";");

            migrationBuilder.CreateTable(
                name: "ParseCorrections",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetailerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParsedItemCount = table.Column<int>(type: "integer", nullable: false),
                    InitiatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParseCorrections", x => x.Id);
                });

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_InventoryItems_ProductName_Trgm\" " +
                "ON inventory.\"InventoryItems\" USING GIN (\"ProductName\" gin_trgm_ops);");

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrections_HouseholdId",
                schema: "inventory",
                table: "ParseCorrections",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrections_HouseholdId_RetailerName",
                schema: "inventory",
                table: "ParseCorrections",
                columns: new[] { "HouseholdId", "RetailerName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParseCorrections",
                schema: "inventory");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ProductName_Trgm",
                schema: "inventory",
                table: "InventoryItems");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_HouseholdId_ProductName",
                schema: "inventory",
                table: "InventoryItems",
                columns: new[] { "HouseholdId", "ProductName" });
        }
    }
}
