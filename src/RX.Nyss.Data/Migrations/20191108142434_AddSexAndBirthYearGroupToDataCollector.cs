using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSexAndBirthYearGroupToDataCollector : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BirthGroupDecade",
                schema: "nyss",
                table: "DataCollectors",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                schema: "nyss",
                table: "DataCollectors",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthGroupDecade",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropColumn(
                name: "Sex",
                schema: "nyss",
                table: "DataCollectors");
        }
    }
}
