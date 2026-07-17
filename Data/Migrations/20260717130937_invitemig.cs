using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class invitemig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerInvitations_AspNetUsers_ServerId",
                table: "ServerInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerInvitations_Servers_ServerId1",
                table: "ServerInvitations");

            migrationBuilder.DropIndex(
                name: "IX_ServerInvitations_ServerId1",
                table: "ServerInvitations");

            migrationBuilder.DropColumn(
                name: "ServerId1",
                table: "ServerInvitations");

            migrationBuilder.CreateIndex(
                name: "IX_ServerInvitations_SenderId",
                table: "ServerInvitations",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerInvitations_AspNetUsers_SenderId",
                table: "ServerInvitations",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerInvitations_Servers_ServerId",
                table: "ServerInvitations",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerInvitations_AspNetUsers_SenderId",
                table: "ServerInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerInvitations_Servers_ServerId",
                table: "ServerInvitations");

            migrationBuilder.DropIndex(
                name: "IX_ServerInvitations_SenderId",
                table: "ServerInvitations");

            migrationBuilder.AddColumn<int>(
                name: "ServerId1",
                table: "ServerInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServerInvitations_ServerId1",
                table: "ServerInvitations",
                column: "ServerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerInvitations_AspNetUsers_ServerId",
                table: "ServerInvitations",
                column: "ServerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerInvitations_Servers_ServerId1",
                table: "ServerInvitations",
                column: "ServerId1",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
