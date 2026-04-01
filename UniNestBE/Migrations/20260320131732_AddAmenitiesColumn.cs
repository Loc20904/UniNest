using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenitiesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Listings");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 26, 30, 698, DateTimeKind.Local).AddTicks(3120), new DateTime(2026, 4, 10, 15, 26, 30, 698, DateTimeKind.Local).AddTicks(3151) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 26, 30, 698, DateTimeKind.Local).AddTicks(3176), new DateTime(2026, 4, 10, 15, 26, 30, 698, DateTimeKind.Local).AddTicks(3177) });
        }
    }
}
