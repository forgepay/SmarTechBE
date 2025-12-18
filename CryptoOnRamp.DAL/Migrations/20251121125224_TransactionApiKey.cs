using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class TransactionApiKey : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ContractApiKey",
            schema: "public",
            table: "Transactions",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ContractUniqueId",
            schema: "public",
            table: "Transactions",
            type: "uuid",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ContractApiKey",
            schema: "public",
            table: "Transactions");

        migrationBuilder.DropColumn(
            name: "ContractUniqueId",
            schema: "public",
            table: "Transactions");
    }
}
