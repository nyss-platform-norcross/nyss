using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddErrorTypeAndCorrectedPropToReports : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CorrectedAt",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectedById",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ErrorType",
                schema: "nyss",
                table: "RawReports",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CorrectedById",
                schema: "nyss",
                table: "Reports",
                column: "CorrectedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_CorrectedById",
                schema: "nyss",
                table: "Reports",
                column: "CorrectedById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE [nyss].[RawReports]
                SET [ErrorType]=12
                WHERE [ReportId] IS NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [nyss].[RawReports]
                SET [ReportId]=NULL
                WHERE [ReportId] IS NOT NULL AND [DataCollectorId] IS NULL
            ");

            migrationBuilder.Sql(@"
                DELETE FROM [nyss].[Reports]
                WHERE [DataCollectorId] IS NULL
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_CorrectedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_CorrectedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorrectedAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CorrectedById",
                schema: "nyss",
                table: "Reports");

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
