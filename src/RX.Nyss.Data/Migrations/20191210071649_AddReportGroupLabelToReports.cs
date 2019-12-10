using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddReportGroupLabelToReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReportGroupLabel",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReceivedAt",
                schema: "nyss",
                table: "Reports",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportGroupLabel",
                schema: "nyss",
                table: "Reports",
                column: "ReportGroupLabel");

            migrationBuilder.Sql("CREATE SPATIAL INDEX IX_Reports_Location ON nyss.Reports(Location)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_ReceivedAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReportGroupLabel",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportGroupLabel",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.Sql("DROP INDEX IX_Reports_Location ON nyss.Reports");
        }
    }
}
