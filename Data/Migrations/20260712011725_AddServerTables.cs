using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Channels",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "Channels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ServerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserServers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserServers_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ServerId",
                table: "Channels",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserServers_ServerId",
                table: "UserServers",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserServers_UserId",
                table: "UserServers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Servers_ServerId",
                table: "Channels",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Servers_ServerId",
                table: "Channels");

            migrationBuilder.DropTable(
                name: "UserServers");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Channels_ServerId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Channels",
                newName: "ChannelId");
        }
    }
}
