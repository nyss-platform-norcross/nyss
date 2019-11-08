using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class ChangeDataCollectorBirthYearPropertyType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthYearGroup",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.AddColumn<int>(
                name: "TenYearBirthGroupStartYear",
                schema: "nyss",
                table: "DataCollectors",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenYearBirthGroupStartYear",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.AddColumn<string>(
                name: "BirthYearGroup",
                schema: "nyss",
                table: "DataCollectors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
