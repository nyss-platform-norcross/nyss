using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSpanishLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "nyss",
                table: "ApplicationLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[] { 3, "Español", "es" });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "ContentLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[] { 3, "Español", "es" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "ApplicationLanguages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
