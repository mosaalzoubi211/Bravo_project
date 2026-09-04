using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bravo.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientReview",
                table: "Tasks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskRating",
                table: "Tasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientReview",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskRating",
                table: "Tasks");
        }
    }
}
