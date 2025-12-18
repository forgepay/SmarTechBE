using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class TelegramUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TelegramUsers",
            schema: "public",
            columns: table => new
            {
                TelegramId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TelegramUsers", x => x.TelegramId);
                table.ForeignKey(
                    name: "FK_TelegramUsers_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "public",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TelegramUsers_UserId",
            schema: "public",
            table: "TelegramUsers",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TelegramUsers",
            schema: "public");
    }
}
