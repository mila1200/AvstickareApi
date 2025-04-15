using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvstickareApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FromLat",
                table: "Trips",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FromLng",
                table: "Trips",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ToLat",
                table: "Trips",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ToLng",
                table: "Trips",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromLat",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FromLng",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ToLat",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ToLng",
                table: "Trips");
        }
    }
}
