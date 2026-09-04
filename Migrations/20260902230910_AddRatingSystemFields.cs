using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bravo.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingSystemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientRating",
                table: "Tasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientRating",
                table: "Tasks");
        }
    }
}
