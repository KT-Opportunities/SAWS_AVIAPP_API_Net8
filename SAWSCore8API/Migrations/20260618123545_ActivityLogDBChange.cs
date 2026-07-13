using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAWSCore8API.Migrations
{
    /// <inheritdoc />
    public partial class ActivityLogDBChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLog",
                columns: table => new
                {
                    activityLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    activityAction = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    activityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    activityDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    createdby_aspnetuserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    createdby_aspnetusername = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    path = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    remoteipaddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsMobile = table.Column<bool>(type: "bit", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OsName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OsVersion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BrowserName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BrowserVersion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClientHints = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isdeleted = table.Column<bool>(type: "bit", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLog", x => x.activityLogId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLog");
        }
    }
}
