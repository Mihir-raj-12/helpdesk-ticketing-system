using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixKbCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KbArticles_AspNetUsers_AuthorUserId",
                table: "KbArticles");

            migrationBuilder.DropForeignKey(
                name: "FK_KbArticles_AspNetUsers_LastUpdatedByUserId",
                table: "KbArticles");

            migrationBuilder.AddForeignKey(
                name: "FK_KbArticles_AspNetUsers_AuthorUserId",
                table: "KbArticles",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KbArticles_AspNetUsers_LastUpdatedByUserId",
                table: "KbArticles",
                column: "LastUpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KbArticles_AspNetUsers_AuthorUserId",
                table: "KbArticles");

            migrationBuilder.DropForeignKey(
                name: "FK_KbArticles_AspNetUsers_LastUpdatedByUserId",
                table: "KbArticles");

            migrationBuilder.AddForeignKey(
                name: "FK_KbArticles_AspNetUsers_AuthorUserId",
                table: "KbArticles",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KbArticles_AspNetUsers_LastUpdatedByUserId",
                table: "KbArticles",
                column: "LastUpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
