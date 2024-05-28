using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactBook_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_AspNetUsers_userId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Activities",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_userId",
                table: "Activities",
                newName: "IX_Activities_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_AspNetUsers_UserId",
                table: "Activities",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_AspNetUsers_UserId",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Activities",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_UserId",
                table: "Activities",
                newName: "IX_Activities_userId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Contacts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_AspNetUsers_userId",
                table: "Activities",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
