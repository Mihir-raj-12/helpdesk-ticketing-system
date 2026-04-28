using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupportEmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessHourStart = table.Column<TimeSpan>(type: "time", nullable: false),
                    BusinessHourEnd = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkingDays = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlaCriticalResolutionHours = table.Column<int>(type: "int", nullable: false),
                    SlaHighResolutionHours = table.Column<int>(type: "int", nullable: false),
                    SlaMediumResolutionHours = table.Column<int>(type: "int", nullable: false),
                    SlaLowResolutionHours = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "BusinessHourEnd", "BusinessHourStart", "SlaCriticalResolutionHours", "SlaHighResolutionHours", "SlaLowResolutionHours", "SlaMediumResolutionHours", "SupportEmailAddress", "SystemName", "WorkingDays" },
                values: new object[] { 1, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 4, 8, 40, 24, "helpdesk@company.com", "Help Desk Ticket Management System", "Monday,Tuesday,Wednesday,Thursday,Friday" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
