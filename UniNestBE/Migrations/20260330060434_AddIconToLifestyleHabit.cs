using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddIconToLifestyleHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "LifestyleHabits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "LifestyleHabits",
                keyColumn: "LifestyleHabitId",
                keyValue: 1,
                column: "Icon",
                value: null);

            migrationBuilder.UpdateData(
                table: "LifestyleHabits",
                keyColumn: "LifestyleHabitId",
                keyValue: 2,
                column: "Icon",
                value: null);

            migrationBuilder.UpdateData(
                table: "LifestyleHabits",
                keyColumn: "LifestyleHabitId",
                keyValue: 3,
                column: "Icon",
                value: null);

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 13, 4, 33, 722, DateTimeKind.Local).AddTicks(3242), new DateTime(2026, 4, 29, 13, 4, 33, 722, DateTimeKind.Local).AddTicks(3249) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 13, 4, 33, 722, DateTimeKind.Local).AddTicks(3257), new DateTime(2026, 4, 29, 13, 4, 33, 722, DateTimeKind.Local).AddTicks(3257) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "LifestyleHabits");

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
    }
}
