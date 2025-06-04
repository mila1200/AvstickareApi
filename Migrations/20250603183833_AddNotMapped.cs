using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvstickareApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNotMapped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Places_FromPlacePlaceId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Places_ToPlacePlaceId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_FromPlacePlaceId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ToPlacePlaceId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FromPlacePlaceId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ToPlacePlaceId",
                table: "Trips");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FromPlacePlaceId",
                table: "Trips",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToPlacePlaceId",
                table: "Trips",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_FromPlacePlaceId",
                table: "Trips",
                column: "FromPlacePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ToPlacePlaceId",
                table: "Trips",
                column: "ToPlacePlaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Places_FromPlacePlaceId",
                table: "Trips",
                column: "FromPlacePlaceId",
                principalTable: "Places",
                principalColumn: "PlaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Places_ToPlacePlaceId",
                table: "Trips",
                column: "ToPlacePlaceId",
                principalTable: "Places",
                principalColumn: "PlaceId");
        }
    }
}
