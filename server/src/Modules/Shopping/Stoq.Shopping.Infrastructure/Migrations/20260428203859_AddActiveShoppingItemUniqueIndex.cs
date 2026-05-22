using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stoq.Shopping.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveShoppingItemUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_ShoppingListItems_HouseholdId_ProductName_Active",
                schema: "shopping",
                table: "ShoppingListItems",
                columns: new[] { "HouseholdId", "ProductName" },
                unique: true,
                filter: "\"IsChecked\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ShoppingListItems_HouseholdId_ProductName_Active",
                schema: "shopping",
                table: "ShoppingListItems");
        }
    }
}
