using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _5.DataAccessLayer_DAL_.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAndCompletionToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ClientEmail",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ClientEmail",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Projects");
        }
    }
}
