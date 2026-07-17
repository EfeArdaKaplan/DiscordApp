using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServerInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Servers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ServerInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    ServerId1 = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerInvitations_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerInvitations_AspNetUsers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerInvitations_Servers_ServerId1",
                        column: x => x.ServerId1,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_InviteCode",
                table: "Servers",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerInvitations_ReceiverId",
                table: "ServerInvitations",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerInvitations_ServerId",
                table: "ServerInvitations",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerInvitations_ServerId1",
                table: "ServerInvitations",
                column: "ServerId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Servers_InviteCode",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Servers");
        }
    }
}
