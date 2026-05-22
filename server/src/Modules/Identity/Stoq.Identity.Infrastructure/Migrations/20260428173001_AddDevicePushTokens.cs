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
            migrationBuilder.RenameTable(
                name: "DevicePushTokens",
                schema: "identity",
                newName: "DevicePushTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DevicePushTokens",
                newName: "DevicePushTokens",
                newSchema: "identity");
        }
    }
}
