using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _SecurityLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalImageUrlToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalImageUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalImageUrl",
                table: "AspNetUsers");
        }
    }
}
