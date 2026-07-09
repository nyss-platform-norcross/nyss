using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateEidsrConfigurationTableAddSexAge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportAgeGroupDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportGenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportAgeGroupDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "ReportGenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration");
        }
    }
}
