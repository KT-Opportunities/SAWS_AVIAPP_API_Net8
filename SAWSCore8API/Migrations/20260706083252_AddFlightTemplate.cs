using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightTemplate",
                columns: table => new
                {
                    flightTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    templateName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    flightNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    departureICAO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    enRouteICAO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    destinationICAO = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    etd = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ete = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    createdby_aspnetuserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    createdby_aspnetusername = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isdeleted = table.Column<bool>(type: "bit", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightTemplate", x => x.flightTemplateId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightTemplate");
        }
    }
}
