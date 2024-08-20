using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isactive",
                table: "userprofile",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "userprofile",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isactive",
                table: "userprofile");

            migrationBuilder.DropColumn(
                name: "username",
                table: "userprofile");
        }
    }
}
