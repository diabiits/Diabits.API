using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diabits.API.Migrations
{
    /// <inheritdoc />
    public partial class InviteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Invites_InviteId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InviteId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "InviteId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InviteId",
                table: "AspNetUsers",
                column: "InviteId",
                unique: true,
                filter: "[InviteId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Invites_InviteId",
                table: "AspNetUsers",
                column: "InviteId",
                principalTable: "Invites",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Invites_InviteId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InviteId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "InviteId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InviteId",
                table: "AspNetUsers",
                column: "InviteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Invites_InviteId",
                table: "AspNetUsers",
                column: "InviteId",
                principalTable: "Invites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
