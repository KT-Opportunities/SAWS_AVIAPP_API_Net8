using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mobilenumber",
                table: "userprofile");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "Package",
                columns: new[] { "packageId", "created_at", "deleted_at", "isdeleted", "name", "price", "updated_at" },
                values: new object[] { 7, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 7);

            migrationBuilder.AddColumn<string>(
                name: "mobilenumber",
                table: "userprofile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
