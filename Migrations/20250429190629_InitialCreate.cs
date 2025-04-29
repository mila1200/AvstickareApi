using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AvstickareApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    AppUserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.AppUserId);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    PlaceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MapServicePlaceId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.PlaceId);
                });

            migrationBuilder.CreateTable(
                name: "FavoritePlaces",
                columns: table => new
                {
                    FavoritePlaceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<string>(type: "text", nullable: false),
                    MapServicePlaceId = table.Column<string>(type: "text", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlaceId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritePlaces", x => x.FavoritePlaceId);
                    table.ForeignKey(
                        name: "FK_FavoritePlaces_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "AppUserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritePlaces_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "PlaceId");
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    TripId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FromPlaceId = table.Column<string>(type: "text", nullable: false),
                    FromPlacePlaceId = table.Column<int>(type: "integer", nullable: true),
                    ToPlaceId = table.Column<string>(type: "text", nullable: false),
                    ToPlacePlaceId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.TripId);
                    table.ForeignKey(
                        name: "FK_Trips_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "AppUserId");
                    table.ForeignKey(
                        name: "FK_Trips_Places_FromPlacePlaceId",
                        column: x => x.FromPlacePlaceId,
                        principalTable: "Places",
                        principalColumn: "PlaceId");
                    table.ForeignKey(
                        name: "FK_Trips_Places_ToPlacePlaceId",
                        column: x => x.ToPlacePlaceId,
                        principalTable: "Places",
                        principalColumn: "PlaceId");
                });

            migrationBuilder.CreateTable(
                name: "TripStops",
                columns: table => new
                {
                    TripStopId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripId = table.Column<int>(type: "integer", nullable: false),
                    MapServicePlaceId = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PlaceId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStops", x => x.TripStopId);
                    table.ForeignKey(
                        name: "FK_TripStops_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "PlaceId");
                    table.ForeignKey(
                        name: "FK_TripStops_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "TripId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Email",
                table: "AppUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_UserName",
                table: "AppUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePlaces_AppUserId",
                table: "FavoritePlaces",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePlaces_PlaceId",
                table: "FavoritePlaces",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_AppUserId",
                table: "Trips",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_FromPlacePlaceId",
                table: "Trips",
                column: "FromPlacePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ToPlacePlaceId",
                table: "Trips",
                column: "ToPlacePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_PlaceId",
                table: "TripStops",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_TripId",
                table: "TripStops",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoritePlaces");

            migrationBuilder.DropTable(
                name: "TripStops");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Places");
        }
    }
}
