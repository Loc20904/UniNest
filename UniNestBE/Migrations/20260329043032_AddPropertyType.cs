using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniNestBE.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyTypeId",
                table: "Listings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PropertyTypes",
                columns: table => new
                {
                    PropertyTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyTypes", x => x.PropertyTypeId);
                });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpireAt", "PropertyTypeId" },
                values: new object[] { new DateTime(2026, 3, 29, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7700), new DateTime(2026, 4, 28, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7709), null });

            migrationBuilder.UpdateData(
                table: "Listings",
                keyColumn: "ListingId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpireAt", "PropertyTypeId" },
                values: new object[] { new DateTime(2026, 3, 29, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7715), new DateTime(2026, 4, 28, 11, 30, 29, 663, DateTimeKind.Local).AddTicks(7715), null });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_PropertyTypeId",
                table: "Listings",
                column: "PropertyTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_PropertyTypes_PropertyTypeId",
                table: "Listings",
                column: "PropertyTypeId",
                principalTable: "PropertyTypes",
                principalColumn: "PropertyTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_PropertyTypes_PropertyTypeId",
                table: "Listings");

            migrationBuilder.DropTable(
                name: "PropertyTypes");

            migrationBuilder.DropIndex(
                name: "IX_Listings_PropertyTypeId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PropertyTypeId",
                table: "Listings");

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
