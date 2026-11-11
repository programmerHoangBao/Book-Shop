using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end.Migrations
{
    /// <inheritdoc />
    public partial class ModifyStoreRenameStoreCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CodeStore",
                table: "Stores",
                newName: "StoreCode");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_CodeStore",
                table: "Stores",
                newName: "IX_Stores_StoreCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StoreCode",
                table: "Stores",
                newName: "CodeStore");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_StoreCode",
                table: "Stores",
                newName: "IX_Stores_CodeStore");
        }
    }
}
