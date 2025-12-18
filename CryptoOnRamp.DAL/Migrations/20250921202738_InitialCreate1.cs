using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class InitialCreate1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:hstore", ",,");

        migrationBuilder.CreateTable(
            name: "FeeSchemes",
            schema: "public",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Type = table.Column<string>(type: "text", nullable: false),
                TargetUserId = table.Column<int>(type: "integer", nullable: true),
                Percent = table.Column<decimal>(type: "numeric", nullable: false),
                UpdatedByUserId = table.Column<int>(type: "integer", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FeeSchemes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Sessions",
            schema: "public",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                Nonce = table.Column<long>(type: "bigint", nullable: false),
                Claims = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false),
                ActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            schema: "public",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                Email = table.Column<string>(type: "text", nullable: true),
                Country = table.Column<string>(type: "text", nullable: true),
                Role = table.Column<string>(type: "text", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: true),
                PasswordResetKey = table.Column<string>(type: "text", nullable: true),
                CreatedById = table.Column<int>(type: "integer", nullable: true),
                Phone = table.Column<string>(type: "text", nullable: true),
                UsdcWallet = table.Column<string>(type: "text", nullable: true),
                IsVerified = table.Column<bool>(type: "boolean", nullable: true),
                RegistrationStep = table.Column<string>(type: "text", nullable: true),
                TelegramId = table.Column<long>(type: "bigint", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Transactions",
            schema: "public",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                Provider = table.Column<string>(type: "text", nullable: false),
                ExternalId = table.Column<string>(type: "text", nullable: true),
                FiatCurrency = table.Column<string>(type: "text", nullable: false),
                FiatAmount = table.Column<decimal>(type: "numeric", nullable: false),
                UserWallet = table.Column<string>(type: "text", nullable: false),
                UniqueWalletAddress = table.Column<string>(type: "text", nullable: false),
                UniquePrivateKey = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                TxHash = table.Column<string>(type: "text", nullable: true),
                ProcessorPercent = table.Column<decimal>(type: "numeric", nullable: false),
                SuperAgentPercent = table.Column<decimal>(type: "numeric", nullable: false),
                AgentPercent = table.Column<decimal>(type: "numeric", nullable: false),
                ProcessorFee = table.Column<decimal>(type: "numeric", nullable: false),
                SuperAgentFee = table.Column<decimal>(type: "numeric", nullable: false),
                AgentFee = table.Column<decimal>(type: "numeric", nullable: false),
                NetPayout = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Transactions_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "public",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_UserId",
            schema: "public",
            table: "Transactions",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "FeeSchemes",
            schema: "public");

        migrationBuilder.DropTable(
            name: "Sessions",
            schema: "public");

        migrationBuilder.DropTable(
            name: "Transactions",
            schema: "public");

        migrationBuilder.DropTable(
            name: "Users",
            schema: "public");
    }
}
