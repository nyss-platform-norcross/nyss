using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateEidsrConfigurationTableAddNewColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportAgeAtLeastFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportAgeBelowFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportGenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.AddColumn<string>(
                name: "ReportCaseCountFemaleAgeAtLeastFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportCaseCountFemaleAgeBelowFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportCaseCountMaleAgeAtLeastFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportCaseCountMaleAgeBelowFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportDataCollectorIdDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportDateDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportGeoLocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportTimeDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportCaseCountFemaleAgeAtLeastFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportCaseCountFemaleAgeBelowFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportCaseCountMaleAgeAtLeastFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportCaseCountMaleAgeBelowFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportDataCollectorIdDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportDateDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportGeoLocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportTimeDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.AddColumn<string>(
                name: "ReportAgeAtLeastFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportAgeBelowFiveDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportGenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);
        }
    }
}
