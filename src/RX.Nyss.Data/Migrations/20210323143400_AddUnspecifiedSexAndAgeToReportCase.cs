using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddUnspecifiedSexAndAgeToReportCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KeptCase_CountUnspecifiedSexAndAge",
                schema: "nyss",
                table: "Reports",
                nullable: true);

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
                name: "KeptCase_CountUnspecifiedSexAndAge",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportedCase_CountUnspecifiedSexAndAge",
                schema: "nyss",
                table: "Reports");
        }
    }
}
