using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddUnspecifiedSexAndAgeToReportCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeptCase_CountFemalesAtLeastFive",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "KeptCase_CountFemalesBelowFive",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "KeptCase_CountMalesAtLeastFive",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "KeptCase_CountMalesBelowFive",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "ReportedCase_CountUnspecifiedSexAndAge",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE r SET r.ReportType = 'Event'
                FROM nyss.Reports AS r
                WHERE r.ReportType='Statement'
            ");

            migrationBuilder.Sql(@"
                UPDATE r set r.ReportedCase_CountUnspecifiedSexAndAge = 0
                FROM nyss.Reports AS r
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE r SET r.ReportType = 'Statement'
                FROM nyss.Reports AS r
                WHERE r.ReportType='Event'
            ");

            migrationBuilder.DropColumn(
                name: "ReportedCase_CountUnspecifiedSexAndAge",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "KeptCase_CountFemalesAtLeastFive",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KeptCase_CountFemalesBelowFive",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KeptCase_CountMalesAtLeastFive",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KeptCase_CountMalesBelowFive",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true);
        }
    }
}
