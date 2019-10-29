using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddCountriesAndNationalSocietyStartDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                schema: "nyss",
                name: "CountryId",
                table: "NationalSocieties",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                schema: "nyss",
                name: "StartDate",
                table: "NationalSocieties",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                schema: "nyss",
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

            migrationBuilder.CreateIndex(
                schema: "nyss",
                name: "IX_NationalSocieties_CountryId",
                table: "NationalSocieties",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                schema: "nyss",
                name: "FK_NationalSocieties_Countries_CountryId",
                table: "NationalSocieties",
                column: "CountryId",
                principalSchema: "nyss",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                schema: "nyss",
                name: "FK_NationalSocieties_Countries_CountryId",
                table: "NationalSocieties");

            migrationBuilder.DropTable(
                schema: "nyss",
                name: "Countries");

            migrationBuilder.DropIndex(
                schema: "nyss",
                name: "IX_NationalSocieties_CountryId",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                schema: "nyss",
                name: "CountryId",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                schema: "nyss",
                name: "StartDate",
                table: "NationalSocieties");
        }
    }
}
