using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations;

/// <inheritdoc />
public partial class SessionId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // First, drop the identity generation
        migrationBuilder.AlterColumn<int>(
            name: "Id",
            schema: "public",
            table: "CheckoutSessions",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer")
            .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        // Then, change the type to text
        migrationBuilder.AlterColumn<string>(
            name: "Id",
            schema: "public",
            table: "CheckoutSessions",
            type: "text",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Change back to integer
        migrationBuilder.AlterColumn<int>(
            name: "Id",
            schema: "public",
            table: "CheckoutSessions",
            type: "integer",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        // Restore the identity generation
        migrationBuilder.AlterColumn<int>(
            name: "Id",
            schema: "public",
            table: "CheckoutSessions",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer")
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
    }
}