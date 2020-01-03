using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class SetProjectHealthRiskIdRequiredInReportsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
