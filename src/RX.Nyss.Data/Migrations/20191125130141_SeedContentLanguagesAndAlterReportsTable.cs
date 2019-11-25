using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class SeedContentLanguagesAndAlterReportsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "ApplicationLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[,]
                {
                    { 1, "English", "en" },
                    { 2, "Français", "fr" }
                });

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

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicationLanguageId",
                value: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "ApplicationLanguages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "ApplicationLanguages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicationLanguageId",
                value: null);

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

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicationLanguageId",
                value: null);
        }
    }
}
