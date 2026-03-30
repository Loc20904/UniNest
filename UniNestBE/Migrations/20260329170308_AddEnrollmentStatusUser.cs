using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentStatusUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentAddress",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentStatus",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificationId",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Major",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YearOfStudy",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CurrentAddress", "DateOfBirth", "EnrollmentStatus", "IdentificationId", "Major", "Nationality", "StudentId", "YearOfStudy" },
                values: new object[] { null, null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EnrollmentStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdentificationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Major",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "YearOfStudy",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 29, 14, 50, 40, 180, DateTimeKind.Local).AddTicks(7646), new DateTime(2026, 4, 28, 14, 50, 40, 180, DateTimeKind.Local).AddTicks(7655) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 29, 14, 50, 40, 180, DateTimeKind.Local).AddTicks(7662), new DateTime(2026, 4, 28, 14, 50, 40, 180, DateTimeKind.Local).AddTicks(7662) });
        }
    }
}
