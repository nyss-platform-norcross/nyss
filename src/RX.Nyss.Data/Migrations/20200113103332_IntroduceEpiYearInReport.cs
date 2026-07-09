using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class IntroduceEpiYearInReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpiYear",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                defaultValue: 2020);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpiYear",
                schema: "nyss",
                table: "Reports");
        }
    }
}
