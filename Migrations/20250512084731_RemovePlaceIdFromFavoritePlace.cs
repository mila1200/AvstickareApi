using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvstickareApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlaceIdFromFavoritePlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoritePlaces_Places_PlaceId",
                table: "FavoritePlaces");

            migrationBuilder.DropIndex(
                name: "IX_FavoritePlaces_PlaceId",
                table: "FavoritePlaces");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "FavoritePlaces");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaceId",
                table: "FavoritePlaces",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePlaces_PlaceId",
                table: "FavoritePlaces",
                column: "PlaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoritePlaces_Places_PlaceId",
                table: "FavoritePlaces",
                column: "PlaceId",
                principalTable: "Places",
                principalColumn: "PlaceId");
        }
    }
}
