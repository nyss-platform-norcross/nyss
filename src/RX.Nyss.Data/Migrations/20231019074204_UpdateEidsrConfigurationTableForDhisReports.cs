using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateEidsrConfigurationTableForDhisReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
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

            migrationBuilder.AddColumn<string>(
                name: "ReportHealthRiskDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportLocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportStatusDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportSuspectedDiseaseDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration",
                column: "NationalSocietyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration");

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

            migrationBuilder.DropColumn(
                name: "ReportHealthRiskDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportLocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportStatusDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportSuspectedDiseaseDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.CreateIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration",
                column: "NationalSocietyId");
        }
    }
}
