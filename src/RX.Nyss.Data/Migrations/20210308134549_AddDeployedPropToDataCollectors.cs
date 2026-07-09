using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddDeployedPropToDataCollectors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deployed",
                schema: "nyss",
                table: "DataCollectors",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE dc SET dc.Deployed = 1
                FROM nyss.DataCollectors AS dc
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deployed",
                schema: "nyss",
                table: "DataCollectors");
        }
    }
}
