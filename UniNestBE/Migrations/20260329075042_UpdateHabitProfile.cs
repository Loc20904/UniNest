using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHabitProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LifestyleHabitLifestyleProfile",
                columns: table => new
                {
                    LifestyleHabitsLifestyleHabitId = table.Column<int>(type: "int", nullable: false),
                    LifestyleProfilesProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifestyleHabitLifestyleProfile", x => new { x.LifestyleHabitsLifestyleHabitId, x.LifestyleProfilesProfileId });
                    table.ForeignKey(
                        name: "FK_LifestyleHabitLifestyleProfile_LifestyleHabits_LifestyleHabitsLifestyleHabitId",
                        column: x => x.LifestyleHabitsLifestyleHabitId,
                        principalTable: "LifestyleHabits",
                        principalColumn: "LifestyleHabitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LifestyleHabitLifestyleProfile_LifestyleProfiles_LifestyleProfilesProfileId",
                        column: x => x.LifestyleProfilesProfileId,
                        principalTable: "LifestyleProfiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_LifestyleHabitLifestyleProfile_LifestyleProfilesProfileId",
                table: "LifestyleHabitLifestyleProfile",
                column: "LifestyleProfilesProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LifestyleHabitLifestyleProfile");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 29, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7700), new DateTime(2026, 4, 28, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7709) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 29, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7715), new DateTime(2026, 4, 28, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7715) });
        }
    }
}
