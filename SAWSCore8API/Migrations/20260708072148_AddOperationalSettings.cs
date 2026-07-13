using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationalSettings",
                columns: table => new
                {
                    OperationalSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PilotName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PilotLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatcherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatcherLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdby_aspnetuserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdby_aspnetusername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isdeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalSettings", x => x.OperationalSettingsId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationalSettings");
        }
    }
}
