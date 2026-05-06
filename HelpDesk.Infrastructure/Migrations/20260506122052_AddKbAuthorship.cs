using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKbAuthorship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Wipe old test data that doesn't have Authors attached
            migrationBuilder.Sql("DELETE FROM KbArticleVersions");
            migrationBuilder.Sql("DELETE FROM KbArticles");

            migrationBuilder.DropForeignKey(
                name: "FK_KbArticleVersions_AspNetUsers_SavedByUserId",
                table: "KbArticleVersions");

            migrationBuilder.DropIndex(
                name: "IX_KbArticleVersions_SavedByUserId",
                table: "KbArticleVersions");

            migrationBuilder.DropColumn(
                name: "SavedByUserId",
                table: "KbArticleVersions");

            migrationBuilder.RenameColumn(
                name: "TitleSnapshot",
                table: "KbArticleVersions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "ContentSnapshot",
                table: "KbArticleVersions",
                newName: "Content");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "KbArticleVersions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "KbArticleVersions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuthorUserId",
                table: "KbArticles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedByUserId",
                table: "KbArticles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_KbArticleVersions_UpdatedByUserId",
                table: "KbArticleVersions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KbArticles_AuthorUserId",
                table: "KbArticles",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KbArticles_LastUpdatedByUserId",
                table: "KbArticles",
                column: "LastUpdatedByUserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_KbArticleVersions_AspNetUsers_UpdatedByUserId",
                table: "KbArticleVersions",
                column: "UpdatedByUserId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_KbArticleVersions_AspNetUsers_UpdatedByUserId",
                table: "KbArticleVersions");

            migrationBuilder.DropIndex(
                name: "IX_KbArticleVersions_UpdatedByUserId",
                table: "KbArticleVersions");

            migrationBuilder.DropIndex(
                name: "IX_KbArticles_AuthorUserId",
                table: "KbArticles");

            migrationBuilder.DropIndex(
                name: "IX_KbArticles_LastUpdatedByUserId",
                table: "KbArticles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "KbArticleVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "KbArticleVersions");

            migrationBuilder.DropColumn(
                name: "AuthorUserId",
                table: "KbArticles");

            migrationBuilder.DropColumn(
                name: "LastUpdatedByUserId",
                table: "KbArticles");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "KbArticleVersions",
                newName: "TitleSnapshot");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "KbArticleVersions",
                newName: "ContentSnapshot");

            migrationBuilder.AddColumn<string>(
                name: "SavedByUserId",
                table: "KbArticleVersions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_KbArticleVersions_SavedByUserId",
                table: "KbArticleVersions",
                column: "SavedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_KbArticleVersions_AspNetUsers_SavedByUserId",
                table: "KbArticleVersions",
                column: "SavedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
