using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParseCorrectionItemMatchingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchStatus",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<Guid>(
                name: "MatchedReferenceProductId",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetailerSku",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                schema: "inventory",
                table: "ParseCorrectionItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrectionItems_Barcode",
                schema: "inventory",
                table: "ParseCorrectionItems",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrectionItems_MatchStatus",
                schema: "inventory",
                table: "ParseCorrectionItems",
                column: "MatchStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParseCorrectionItems_Barcode",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropIndex(
                name: "IX_ParseCorrectionItems_MatchStatus",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "Barcode",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "Discount",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "MatchStatus",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "MatchedReferenceProductId",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "RetailerSku",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                schema: "inventory",
                table: "ParseCorrectionItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                schema: "inventory",
                table: "ParseCorrectionItems");
        }
    }
}
