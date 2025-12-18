using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class AddDefaultLanguageToUsers1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProcessorFee",
            schema: "public",
            table: "Transactions");

        migrationBuilder.DropColumn(
            name: "ProcessorPercent",
            schema: "public",
            table: "Transactions");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "ProcessorFee",
            schema: "public",
            table: "Transactions",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "ProcessorPercent",
            schema: "public",
            table: "Transactions",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);
    }
}
