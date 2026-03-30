using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestAndHubToLifestyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestFrequency",
                table: "LifestyleProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredDistricts",
                table: "LifestyleProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 0, 25, 5, 274, DateTimeKind.Local).AddTicks(2596), new DateTime(2026, 4, 29, 0, 25, 5, 274, DateTimeKind.Local).AddTicks(2608) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 0, 25, 5, 274, DateTimeKind.Local).AddTicks(2624), new DateTime(2026, 4, 29, 0, 25, 5, 274, DateTimeKind.Local).AddTicks(2625) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestFrequency",
                table: "LifestyleProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredDistricts",
                table: "LifestyleProfiles");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 0, 3, 7, 949, DateTimeKind.Local).AddTicks(4182), new DateTime(2026, 4, 29, 0, 3, 7, 949, DateTimeKind.Local).AddTicks(4193) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 0, 3, 7, 949, DateTimeKind.Local).AddTicks(4209), new DateTime(2026, 4, 29, 0, 3, 7, 949, DateTimeKind.Local).AddTicks(4209) });
        }
    }
}
