using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class AddCryptoCurrencyField : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CryptoAmount",
            schema: "public",
            table: "Transactions",
            type: "numeric",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CryptoCurrency",
            schema: "public",
            table: "Transactions",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CryptoAmount",
            schema: "public",
            table: "Transactions");

        migrationBuilder.DropColumn(
            name: "CryptoCurrency",
            schema: "public",
            table: "Transactions");
    }
}
