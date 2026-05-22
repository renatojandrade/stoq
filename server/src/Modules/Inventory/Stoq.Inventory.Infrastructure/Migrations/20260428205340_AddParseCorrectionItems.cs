using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParseCorrectionItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParseCorrectionItems",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParseCorrectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParseCorrectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParseCorrectionItems_ParseCorrections_ParseCorrectionId",
                        column: x => x.ParseCorrectionId,
                        principalSchema: "inventory",
                        principalTable: "ParseCorrections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrectionItems_ParseCorrectionId",
                schema: "inventory",
                table: "ParseCorrectionItems",
                column: "ParseCorrectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParseCorrectionItems",
                schema: "inventory");
        }
    }
}
