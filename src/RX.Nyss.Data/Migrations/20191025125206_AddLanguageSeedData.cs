using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddLanguageSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "nyss",
                table: "ContentLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[] { 1, "English", "EN" });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "ContentLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[] { 2, "Français", "FR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "ContentLanguages",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
