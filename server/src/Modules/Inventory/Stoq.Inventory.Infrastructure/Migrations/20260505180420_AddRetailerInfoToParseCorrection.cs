using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRetailerInfoToParseCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RetailerCity",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetailerCnpj",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetailerState",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetailerStreet",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetailerTradeName",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetailerZipCode",
                schema: "inventory",
                table: "ParseCorrections",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetailerCity",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "RetailerCnpj",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "RetailerState",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "RetailerStreet",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "RetailerTradeName",
                schema: "inventory",
                table: "ParseCorrections");

            migrationBuilder.DropColumn(
                name: "RetailerZipCode",
                schema: "inventory",
                table: "ParseCorrections");
        }
    }
}
