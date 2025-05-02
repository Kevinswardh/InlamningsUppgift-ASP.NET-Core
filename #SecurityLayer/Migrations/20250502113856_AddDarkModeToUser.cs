using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _SecurityLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDarkModeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDarkModeEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDarkModeEnabled",
                table: "AspNetUsers");
        }
    }
}
