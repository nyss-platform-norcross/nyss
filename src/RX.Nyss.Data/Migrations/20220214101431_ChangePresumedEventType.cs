using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class ChangePresumedEventType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "CasePositiveLab");

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "CaseNegativeLab");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "PresumedCasePositiveLab");

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "PresumedCaseNegativeLab");
        }
    }
}
