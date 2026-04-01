using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PremiumExpiryDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 31, 16, 55, 48, 876, DateTimeKind.Local).AddTicks(9779), new DateTime(2026, 4, 30, 16, 55, 48, 876, DateTimeKind.Local).AddTicks(9779) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 31, 16, 55, 48, 876, DateTimeKind.Local).AddTicks(9788), new DateTime(2026, 4, 30, 16, 55, 48, 876, DateTimeKind.Local).AddTicks(9788) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "IsPremium", "PremiumExpiryDate" },
                values: new object[] { true, new DateTime(2027, 3, 31, 16, 55, 48, 876, DateTimeKind.Local).AddTicks(9721) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CurrentAddress", "DateOfBirth", "Email", "EnrollmentStatus", "FullName", "Gender", "IdentificationId", "IsBanned", "IsOnline", "IsPremium", "IsVerified", "LastActiveAt", "Major", "Nationality", "PasswordHash", "PhoneNumber", "PremiumExpiryDate", "ResetPasswordToken", "ResetPasswordTokenExpiry", "Role", "StudentAvatar", "StudentId", "UniversityId", "WarningCount", "YearOfStudy" },
                values: new object[] { 1001, null, null, "premium@domain.com", null, "Premium User", false, null, false, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "hashed_password", null, new DateTime(2026, 4, 30, 9, 55, 48, 876, DateTimeKind.Utc).AddTicks(9949), null, null, "user", null, null, null, 0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1001);

            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PremiumExpiryDate",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 20, 47, 6, 276, DateTimeKind.Local).AddTicks(6121), new DateTime(2026, 4, 29, 20, 47, 6, 276, DateTimeKind.Local).AddTicks(6132) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 30, 20, 47, 6, 276, DateTimeKind.Local).AddTicks(6139), new DateTime(2026, 4, 29, 20, 47, 6, 276, DateTimeKind.Local).AddTicks(6139) });
        }
    }
}
