using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class AddDefaultLanguageToUsers3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Payouts",
            schema: "public",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TransactionId = table.Column<int>(type: "integer", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                ToWallet = table.Column<string>(type: "text", nullable: false),
                Amount = table.Column<string>(type: "text", nullable: false),
                CreatetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatetBy = table.Column<string>(type: "text", nullable: false),
                TxHash = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payouts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payouts_Transactions_TransactionId",
                    column: x => x.TransactionId,
                    principalSchema: "public",
                    principalTable: "Transactions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CheckoutSessions_TransactionId",
            schema: "public",
            table: "CheckoutSessions",
            column: "TransactionId");

        migrationBuilder.CreateIndex(
            name: "IX_Payouts_TransactionId",
            schema: "public",
            table: "Payouts",
            column: "TransactionId");

        migrationBuilder.AddForeignKey(
            name: "FK_CheckoutSessions_Transactions_TransactionId",
            schema: "public",
            table: "CheckoutSessions",
            column: "TransactionId",
            principalSchema: "public",
            principalTable: "Transactions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CheckoutSessions_Transactions_TransactionId",
            schema: "public",
            table: "CheckoutSessions");

        migrationBuilder.DropTable(
            name: "Payouts",
            schema: "public");

        migrationBuilder.DropIndex(
            name: "IX_CheckoutSessions_TransactionId",
            schema: "public",
            table: "CheckoutSessions");
    }
}
