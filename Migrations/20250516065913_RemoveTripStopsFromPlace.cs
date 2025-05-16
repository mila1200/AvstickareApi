using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvstickareApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTripStopsFromPlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripStops_Places_PlaceId",
                table: "TripStops");

            migrationBuilder.DropIndex(
                name: "IX_TripStops_PlaceId",
                table: "TripStops");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "TripStops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaceId",
                table: "TripStops",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_PlaceId",
                table: "TripStops",
                column: "PlaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_TripStops_Places_PlaceId",
                table: "TripStops",
                column: "PlaceId",
                principalTable: "Places",
                principalColumn: "PlaceId");
        }
    }
}
