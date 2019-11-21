using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class ChangeProjectHealthRiskRelationInReportToNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
