using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddIsInTrainingModeColumnToDataCollectorsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsInTrainingMode",
                schema: "nyss",
                table: "DataCollectors",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInTrainingMode",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
