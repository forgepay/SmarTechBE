using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UserDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Users");
        }
    }
}
