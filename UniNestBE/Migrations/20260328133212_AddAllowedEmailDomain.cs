using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedEmailDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedEmailDomains",
                columns: table => new
                {
                    DomainId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedEmailDomains", x => x.DomainId);
                });

            migrationBuilder.InsertData(
                table: "AllowedEmailDomains",
                columns: new[] { "DomainId", "Description", "DomainName" },
                values: new object[] { 1, "Email Sinh viên Toàn quốc", "edu.vn" });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 28, 20, 32, 10, 986, DateTimeKind.Local).AddTicks(6466), new DateTime(2026, 4, 27, 20, 32, 10, 986, DateTimeKind.Local).AddTicks(6478) });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt" },
                values: new object[] { new DateTime(2026, 3, 28, 20, 32, 10, 986, DateTimeKind.Local).AddTicks(6490), new DateTime(2026, 4, 27, 20, 32, 10, 986, DateTimeKind.Local).AddTicks(6490) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedEmailDomains");

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
