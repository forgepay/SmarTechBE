using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoOnRamp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FilteredIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                schema: "public",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "public",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"DeletedAt\" is null");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                schema: "public",
                table: "Users",
                column: "Name",
                unique: true,
                filter: "\"DeletedAt\" is null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                schema: "public",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                schema: "public",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                schema: "public",
                table: "Users",
                column: "Name",
                unique: true);
        }
    }
}
