using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subscription_status",
                table: "Subscription");

            migrationBuilder.AddColumn<bool>(
                name: "isactive",
                table: "Subscription",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isactive",
                table: "Subscription");

            migrationBuilder.AddColumn<string>(
                name: "subscription_status",
                table: "Subscription",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
