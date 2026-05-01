using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEscalationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalationAcknowledgedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalationReason",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscalatedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EscalationAcknowledgedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EscalationReason",
                table: "Tickets");
        }
    }
}
