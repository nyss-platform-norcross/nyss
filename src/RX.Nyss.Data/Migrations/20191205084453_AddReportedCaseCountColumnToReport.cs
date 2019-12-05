using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddReportedCaseCountColumnToReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportedCaseCount",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportedCaseCount",
                schema: "nyss",
                table: "Reports");
        }
    }
}
