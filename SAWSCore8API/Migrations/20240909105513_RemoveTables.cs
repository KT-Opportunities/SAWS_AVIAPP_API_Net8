using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceProduct");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.AlterColumn<string>(
                name: "file_origname",
                table: "DocFeedback",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Package",
                columns: new[] { "packageId", "created_at", "deleted_at", "isdeleted", "name", "price", "updated_at" },
                values: new object[] { 7, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified), null, false, "Admin monthly Regulated", 0.00m, new DateTime(2024, 6, 13, 12, 56, 53, 177, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Package",
                keyColumn: "packageId",
                keyValue: 7);

            migrationBuilder.AlterColumn<string>(
                name: "file_origname",
                table: "DocFeedback",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    serviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isdeleted = table.Column<bool>(type: "bit", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    packageId = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.serviceId);
                    table.ForeignKey(
                        name: "FK_Service_Package_packageId",
                        column: x => x.packageId,
                        principalTable: "Package",
                        principalColumn: "packageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProduct",
                columns: table => new
                {
                    serviceProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isdeleted = table.Column<bool>(type: "bit", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    serviceId = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProduct", x => x.serviceProductId);
                    table.ForeignKey(
                        name: "FK_ServiceProduct_Service_serviceId",
                        column: x => x.serviceId,
                        principalTable: "Service",
                        principalColumn: "serviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Service_packageId",
                table: "Service",
                column: "packageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProduct_serviceId",
                table: "ServiceProduct",
                column: "serviceId");
        }
    }
}
