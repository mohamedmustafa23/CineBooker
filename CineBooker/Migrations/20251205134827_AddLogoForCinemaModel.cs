using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineBooker.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoForCinemaModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Cinemas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Cinemas");
        }
    }
}
