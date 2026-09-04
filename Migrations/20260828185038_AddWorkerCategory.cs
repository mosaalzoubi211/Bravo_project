using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bravo.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "AppUser",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUser_CategoryId",
                table: "AppUser",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUser_Categories_CategoryId",
                table: "AppUser",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUser_Categories_CategoryId",
                table: "AppUser");

            migrationBuilder.DropIndex(
                name: "IX_AppUser_CategoryId",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "AppUser");
        }
    }
}
