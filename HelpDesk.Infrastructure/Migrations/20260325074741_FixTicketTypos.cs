using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTicketTypos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedTOUserId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "AssignedTOUserId",
                table: "Tickets",
                newName: "AssignedToUserId");

            migrationBuilder.RenameColumn(
                name: "Dscription",
                table: "Tickets",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedTOUserId",
                table: "Tickets",
                newName: "IX_Tickets_AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedToUserId",
                table: "Tickets",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedToUserId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId",
                table: "Tickets",
                newName: "AssignedTOUserId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Tickets",
                newName: "Dscription");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedToUserId",
                table: "Tickets",
                newName: "IX_Tickets_AssignedTOUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedTOUserId",
                table: "Tickets",
                column: "AssignedTOUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
