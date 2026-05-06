using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase1SchemaOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "RecurringTickets",
                newName: "TicketTitle");

            migrationBuilder.RenameColumn(
                name: "RaisedByUserId",
                table: "RecurringTickets",
                newName: "RaiseOnBehalfOfUserId");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "RecurringTickets",
                newName: "RecurrencePattern");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tickets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AffectedAsset",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "RelatedTicketId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SlaClosedWithinSla",
                table: "Tickets",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoUrl",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultTimeZone",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SessionTimeoutMinutes",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextRunDate",
                table: "RecurringTickets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "AssignToUserId",
                table: "RecurringTickets",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRunCount",
                table: "RecurringTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "RecurringTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxOccurrences",
                table: "RecurringTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "RecurringTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "RecurringTickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "RecurringTickets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActorEmail",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActorName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActorRole",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NotifyOnTicketCreated = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnTicketAssigned = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnStatusChanged = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnNewComment = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnTicketClosed = table.Column<bool>(type: "bit", nullable: false),
                    OptOutCsatSurveys = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CompanyLogoUrl", "DefaultTimeZone", "SessionTimeoutMinutes" },
                values: new object[] { null, "UTC", 60 });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DepartmentId",
                table: "Tickets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RelatedTicketId",
                table: "Tickets",
                column: "RelatedTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTickets_AssignToUserId",
                table: "RecurringTickets",
                column: "AssignToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTickets_RaiseOnBehalfOfUserId",
                table: "RecurringTickets",
                column: "RaiseOnBehalfOfUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTickets_AspNetUsers_AssignToUserId",
                table: "RecurringTickets",
                column: "AssignToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTickets_AspNetUsers_RaiseOnBehalfOfUserId",
                table: "RecurringTickets",
                column: "RaiseOnBehalfOfUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Departments_DepartmentId",
                table: "Tickets",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Tickets_RelatedTicketId",
                table: "Tickets",
                column: "RelatedTicketId",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTickets_AspNetUsers_AssignToUserId",
                table: "RecurringTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTickets_AspNetUsers_RaiseOnBehalfOfUserId",
                table: "RecurringTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Departments_DepartmentId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Tickets_RelatedTicketId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DepartmentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_RelatedTicketId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_RecurringTickets_AssignToUserId",
                table: "RecurringTickets");

            migrationBuilder.DropIndex(
                name: "IX_RecurringTickets_RaiseOnBehalfOfUserId",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "AffectedAsset",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RelatedTicketId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SlaClosedWithinSla",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CompanyLogoUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DefaultTimeZone",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SessionTimeoutMinutes",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "AssignToUserId",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "CurrentRunCount",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "MaxOccurrences",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "RecurringTickets");

            migrationBuilder.DropColumn(
                name: "ActorEmail",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ActorName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ActorRole",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "TicketTitle",
                table: "RecurringTickets",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "RecurrencePattern",
                table: "RecurringTickets",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "RaiseOnBehalfOfUserId",
                table: "RecurringTickets",
                newName: "RaisedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextRunDate",
                table: "RecurringTickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
