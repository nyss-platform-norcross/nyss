using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class RemovedMarkedAsError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update nyss.RawReports
    set ErrorType = 12 --Other
    ,ReportId = null
from nyss.RawReports RR
    inner join nyss.Reports R on R.Id = RR.ReportId
where R.MarkedAsError = 1
");

            migrationBuilder.DropColumn(
                name: "MarkedAsError",
                schema: "nyss",
                table: "Reports");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarkedAsError",
                schema: "nyss",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
