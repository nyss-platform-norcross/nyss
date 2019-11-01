using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class MoveHealthRiskNameAndAddLanguageContents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nyss");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "nyss",
                table: "HealthRisks");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "nyss",
                table: "HealthRiskLanguageContents");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "HealthRisks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
