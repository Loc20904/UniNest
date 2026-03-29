using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentVerificationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentVerificationRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FrontIdImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BackIdImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CurrentSemester = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentVerificationRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_StudentVerificationRequests_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentVerificationRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 2, 37, 17, 189, DateTimeKind.Local).AddTicks(4873));

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 2, 37, 17, 189, DateTimeKind.Local).AddTicks(4888));

            migrationBuilder.CreateIndex(
                name: "IX_StudentVerificationRequests_ReviewedByAdminId",
                table: "StudentVerificationRequests",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentVerificationRequests_UserId",
                table: "StudentVerificationRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentVerificationRequests");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 17, 22, 42, 261, DateTimeKind.Local).AddTicks(3671));

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 21, 17, 22, 42, 261, DateTimeKind.Local).AddTicks(3689));
        }
    }
}
