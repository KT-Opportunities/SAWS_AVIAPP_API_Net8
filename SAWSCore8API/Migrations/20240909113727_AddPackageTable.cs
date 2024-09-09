using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    packageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isdeleted = table.Column<bool>(type: "bit", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.packageId);
                });

            migrationBuilder.InsertData(
                table: "Package",
                columns: new[] { "packageId", "created_at", "deleted_at", "isdeleted", "name", "price", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified), null, false, "Free", 0.00m, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified), null, false, "monthly Premium", 180.00m, new DateTime(2024, 2, 23, 12, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2024, 6, 13, 12, 49, 49, 577, DateTimeKind.Unspecified), null, false, "monthly Regulated", 380.00m, new DateTime(2024, 6, 13, 12, 49, 49, 577, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "annually Premium", 2160.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "annually Regulated", 4560.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin monthly Regulated", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 8, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin annually Regulated", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 9, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin monthly Premium", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) },
                    { 10, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin annually Premium", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Package");
        }
    }
}
