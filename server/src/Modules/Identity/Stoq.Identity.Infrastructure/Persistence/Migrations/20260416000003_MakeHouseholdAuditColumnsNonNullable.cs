using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeHouseholdAuditColumnsNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill any existing rows that have NULL audit columns.
            migrationBuilder.Sql("""
                UPDATE identity."Households"
                SET "UpdatedAt" = "CreatedAt",
                    "UpdatedBy" = "CreatedBy"
                WHERE "UpdatedAt" IS NULL OR "UpdatedBy" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                schema: "identity",
                table: "Households",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "identity",
                table: "Households",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                schema: "identity",
                table: "Households",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "identity",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }
    }
}
