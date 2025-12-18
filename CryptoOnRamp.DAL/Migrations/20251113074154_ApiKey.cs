using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class ApiKey : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ApiKeyHash",
            schema: "public",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ApiKeyName",
            schema: "public",
            table: "Users",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ApiKeyHash",
            schema: "public",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "ApiKeyName",
            schema: "public",
            table: "Users");
    }
}
