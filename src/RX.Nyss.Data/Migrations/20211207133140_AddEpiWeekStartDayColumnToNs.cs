using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddEpiWeekStartDayColumnToNs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpiWeekStartDay",
                schema: "nyss",
                table: "NationalSocieties",
                type: "int",
                defaultValue: 0,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpiWeekStartDay",
                schema: "nyss",
                table: "NationalSocieties");
        }
    }
}
