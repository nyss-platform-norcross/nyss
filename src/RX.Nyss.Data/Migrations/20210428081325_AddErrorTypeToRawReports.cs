using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddErrorTypeToRawReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ErrorType",
                schema: "nyss",
                table: "RawReports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorType",
                schema: "nyss",
                table: "RawReports");

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
