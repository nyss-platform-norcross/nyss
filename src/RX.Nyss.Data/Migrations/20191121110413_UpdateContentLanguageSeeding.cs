using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateContentLanguageSeeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanguageCode",
                value: "en");

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 2,
                column: "LanguageCode",
                value: "fr");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanguageCode",
                value: "EN");

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 2,
                column: "LanguageCode",
                value: "FR");
        }
    }
}
