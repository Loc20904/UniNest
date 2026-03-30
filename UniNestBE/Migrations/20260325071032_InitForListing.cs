using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class InitForListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 25, 14, 10, 30, 287, DateTimeKind.Local).AddTicks(5666), new DateTime(2026, 4, 24, 14, 10, 30, 287, DateTimeKind.Local).AddTicks(5676) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 25, 14, 10, 30, 287, DateTimeKind.Local).AddTicks(5689), new DateTime(2026, 4, 24, 14, 10, 30, 287, DateTimeKind.Local).AddTicks(5689) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 22, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(5975), new DateTime(2026, 4, 21, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(5992) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 22, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(6010), new DateTime(2026, 4, 21, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(6010) });
        }
    }
}
