using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddListingApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Listings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "ApprovalStatus", "CreatedAt", "ExpireAt" },
                values: new object[] { "Pending", new DateTime(2026, 3, 22, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(5975), new DateTime(2026, 4, 21, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(5992) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "ApprovalStatus", "CreatedAt", "ExpireAt" },
                values: new object[] { "Pending", new DateTime(2026, 3, 22, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(6010), new DateTime(2026, 4, 21, 19, 15, 29, 953, DateTimeKind.Local).AddTicks(6010) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Listings");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 21, 17, 18, 3, 487, DateTimeKind.Local).AddTicks(2479), new DateTime(2026, 4, 20, 17, 18, 3, 487, DateTimeKind.Local).AddTicks(2493) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 21, 17, 18, 3, 487, DateTimeKind.Local).AddTicks(2505), new DateTime(2026, 4, 20, 17, 18, 3, 487, DateTimeKind.Local).AddTicks(2506) });
        }
    }
}
