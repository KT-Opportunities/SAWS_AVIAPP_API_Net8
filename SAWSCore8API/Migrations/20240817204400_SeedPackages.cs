using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class SeedPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Package",
                columns: new[] { "packageId", "created_at", "deleted_at", "isdeleted", "name", "price", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified), null, false, "monthly Free", 0.00m, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified), null, false, "monthly Premium", 180.00m, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2024, 6, 13, 12, 49, 49, 577, DateTimeKind.Unspecified), null, false, "monthly Regulated", 380.00m, new DateTime(2024, 6, 13, 12, 49, 49, 577, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "annually Free", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "annually Premium", 2160.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "annually Regulated", 4560.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 6);
        }
    }
}
