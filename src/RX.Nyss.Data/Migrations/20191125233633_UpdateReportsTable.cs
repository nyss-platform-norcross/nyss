using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateReportsTable : Migration
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

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geography");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTraining",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
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

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                schema: "nyss",
                table: "Reports",
                type: "geography",
                nullable: false,
                oldClrType: typeof(Point),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTraining",
                schema: "nyss",
                table: "Reports",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
