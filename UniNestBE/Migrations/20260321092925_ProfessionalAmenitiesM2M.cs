using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionalAmenitiesM2M : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Listings");

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.AmenityId);
                });

            migrationBuilder.CreateTable(
                name: "AmenityListing",
                columns: table => new
                {
                    AmenitiesAmenityId = table.Column<int>(type: "int", nullable: false),
                    ListingsListingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmenityListing", x => new { x.AmenitiesAmenityId, x.ListingsListingId });
                    table.ForeignKey(
                        name: "FK_AmenityListing_Amenities_AmenitiesAmenityId",
                        column: x => x.AmenitiesAmenityId,
                        principalTable: "Amenities",
                        principalColumn: "AmenityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AmenityListing_Listings_ListingsListingId",
                        column: x => x.ListingsListingId,
                        principalTable: "Listings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "AmenityId", "Icon", "Name" },
                values: new object[,]
                {
                    { 1, "wifi", "Wi-Fi" },
                    { 2, "ac_unit", "Air Conditioning" },
                    { 3, "bathtub", "Private Bath" },
                    { 4, "directions_car", "Parking" },
                    { 5, "kitchen", "Kitchen" },
                    { 6, "local_laundry_service", "Laundry" }
                });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 21, 16, 29, 23, 844, DateTimeKind.Local).AddTicks(9515), new DateTime(2026, 4, 20, 16, 29, 23, 844, DateTimeKind.Local).AddTicks(9530) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 21, 16, 29, 23, 844, DateTimeKind.Local).AddTicks(9543), new DateTime(2026, 4, 20, 16, 29, 23, 844, DateTimeKind.Local).AddTicks(9543) });

            migrationBuilder.CreateIndex(
                name: "IX_AmenityListing_ListingsListingId",
                table: "AmenityListing",
                column: "ListingsListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmenityListing");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "Listings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "Amenities", "CreatedAt", "ExpireAt" },
                values: new object[] { "", new DateTime(2026, 3, 20, 20, 17, 30, 845, DateTimeKind.Local).AddTicks(8520), new DateTime(2026, 4, 19, 20, 17, 30, 845, DateTimeKind.Local).AddTicks(8539) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "Amenities", "CreatedAt", "ExpireAt" },
                values: new object[] { "", new DateTime(2026, 3, 20, 20, 17, 30, 845, DateTimeKind.Local).AddTicks(8550), new DateTime(2026, 4, 19, 20, 17, 30, 845, DateTimeKind.Local).AddTicks(8550) });
        }
    }
}
