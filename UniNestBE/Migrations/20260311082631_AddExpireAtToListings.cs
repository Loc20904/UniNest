using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddExpireAtToListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireAt",
                table: "Listings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpireAt",
                table: "Listings");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 24, 20, 57, 48, 511, DateTimeKind.Local).AddTicks(1096));

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 24, 20, 57, 48, 511, DateTimeKind.Local).AddTicks(1111));
        }
    }
}
