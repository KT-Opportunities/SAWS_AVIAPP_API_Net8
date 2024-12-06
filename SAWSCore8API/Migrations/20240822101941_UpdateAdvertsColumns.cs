using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdvertsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 7);

            migrationBuilder.AlterColumn<bool>(
                name: "ispublished",
                table: "Advert",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 1,
                column: "name",
                value: "Free");

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 4,
                columns: new[] { "name", "price" },
                values: new object[] { "annually Premium", 2160.00m });

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 5,
                columns: new[] { "name", "price" },
                values: new object[] { "annually Regulated", 4560.00m });

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 6,
                columns: new[] { "name", "price" },
                values: new object[] { "Admin", 0.00m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "ispublished",
                table: "Advert",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 1,
                column: "name",
                value: "monthly Free");

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 4,
                columns: new[] { "name", "price" },
                values: new object[] { "annually Free", 0.00m });

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 5,
                columns: new[] { "name", "price" },
                values: new object[] { "annually Premium", 2160.00m });

            migrationBuilder.UpdateData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 6,
                columns: new[] { "name", "price" },
                values: new object[] { "annually Regulated", 4560.00m });

            migrationBuilder.InsertData(
                table: "Package",
                columns: new[] { "packageId", "created_at", "deleted_at", "isdeleted", "name", "price", "updated_at" },
                values: new object[] { 7, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) });
        }
    }
}
