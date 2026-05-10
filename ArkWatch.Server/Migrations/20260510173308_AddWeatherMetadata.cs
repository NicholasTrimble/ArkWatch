using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArkWatch.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Expiration",
                table: "StoredAlerts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HailSize",
                table: "StoredAlerts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WindSpeed",
                table: "StoredAlerts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "StoredAlerts");

            migrationBuilder.DropColumn(
                name: "HailSize",
                table: "StoredAlerts");

            migrationBuilder.DropColumn(
                name: "WindSpeed",
                table: "StoredAlerts");
        }
    }
}
