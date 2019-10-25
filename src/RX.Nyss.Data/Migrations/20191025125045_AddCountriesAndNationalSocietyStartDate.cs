using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddCountriesAndNationalSocietyStartDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "NationalSocieties",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "NationalSocieties",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ContentLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[] { 1, "English", "EN" });

            migrationBuilder.InsertData(
                table: "ContentLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[] { 2, "Français", "FR" });

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_CountryId",
                table: "NationalSocieties",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_NationalSocieties_Countries_CountryId",
                table: "NationalSocieties",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NationalSocieties_Countries_CountryId",
                table: "NationalSocieties");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_CountryId",
                table: "NationalSocieties");

            migrationBuilder.DeleteData(
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "NationalSocieties");
        }
    }
}
