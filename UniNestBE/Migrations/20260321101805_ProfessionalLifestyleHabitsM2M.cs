using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class ProfessionalLifestyleHabitsM2M : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LifestyleHabits",
                columns: table => new
                {
                    LifestyleHabitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifestyleHabits", x => x.LifestyleHabitId);
                });

            migrationBuilder.CreateTable(
                name: "LifestyleHabitListing",
                columns: table => new
                {
                    LifestyleHabitsLifestyleHabitId = table.Column<int>(type: "int", nullable: false),
                    ListingsListingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifestyleHabitListing", x => new { x.LifestyleHabitsLifestyleHabitId, x.ListingsListingId });
                    table.ForeignKey(
                        name: "FK_LifestyleHabitListing_LifestyleHabits_LifestyleHabitsLifestyleHabitId",
                        column: x => x.LifestyleHabitsLifestyleHabitId,
                        principalTable: "LifestyleHabits",
                        principalColumn: "LifestyleHabitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LifestyleHabitListing_Listings_ListingsListingId",
                        column: x => x.ListingsListingId,
                        principalTable: "Listings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LifestyleHabits",
                columns: new[] { "LifestyleHabitId", "Name" },
                values: new object[,]
                {
                    { 1, "Non-smoker only" },
                    { 2, "Pet friendly" },
                    { 3, "Late-night studying" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_LifestyleHabitListing_ListingsListingId",
                table: "LifestyleHabitListing",
                column: "ListingsListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LifestyleHabitListing");

            migrationBuilder.DropTable(
                name: "LifestyleHabits");

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
        }
    }
}
