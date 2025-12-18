using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class WebHookUrl : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "WebhookAuthorizationToken",
            schema: "public",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WebhookUrl",
            schema: "public",
            table: "Users",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "WebhookAuthorizationToken",
            schema: "public",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "WebhookUrl",
            schema: "public",
            table: "Users");
    }
}
