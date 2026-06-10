using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePushTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevicePushTokens",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePushTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevicePushTokens_Token",
                schema: "identity",
                table: "DevicePushTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DevicePushTokens_UserId",
                schema: "identity",
                table: "DevicePushTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DevicePushTokens", schema: "identity");
        }
    }
}
