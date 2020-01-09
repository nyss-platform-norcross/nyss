using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddMarkedAsErrorFlagToReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarkedAsError",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedAsError",
                schema: "nyss",
                table: "Reports");
        }
    }
}
