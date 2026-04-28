using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Tickets",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Comments",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "AuditLogs",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "AuditDetails",
                newName: "IsActive");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DepartmentHeadId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_AspNetUsers_DepartmentHeadId",
                        column: x => x.DepartmentHeadId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DepartmentHeadId", "IsActive", "Name" },
                values: new object[] { 1, null, true, "General" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Tickets",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Comments",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Categories",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AuditLogs",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AuditDetails",
                newName: "isActive");
        }
    }
}
