using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorParseCorrectionForAsyncFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ParsedAt",
                schema: "inventory",
                table: "ParseCorrections",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "inventory",
                table: "ParseCorrections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmittedAt",
                schema: "inventory",
                table: "ParseCorrections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeUrl",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "inventory",
                table: "ParseCorrections",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrections_InitiatedByUserId_Status",
                schema: "inventory",
                table: "ParseCorrections",
                columns: new[] { "InitiatedByUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ParseCorrections_Status",
                schema: "inventory",
                table: "ParseCorrections",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParseCorrections_InitiatedByUserId_Status",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropIndex(
                name: "IX_ParseCorrections_Status",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "EmittedAt",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ParsedAt",
                schema: "inventory",
                table: "ParseCorrections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
